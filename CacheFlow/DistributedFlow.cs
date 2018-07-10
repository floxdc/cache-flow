using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DistributedFlow : ICacheFlow
    {
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow> logger, IOptions<FlowOptions> options)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _options = options.Value;

            CompositeResolver.RegisterAndSetAsDefault(ImmutableCollectionResolver.Instance, StandardResolver.Instance);
        }


        public async Task<T> GetValueAsync<T>(string key)
        {
            var bytes = await GetFromCacheAsync(key);
            if (bytes is null)
            {
                _logger.Miss(key);
                return default;
            }

            var value = MessagePackSerializer.Deserialize<T>(bytes);
            if (!await RefreshCacheAsync(key))
                return default;

            _logger.Hit(key);
            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
        {
            var isCached = TryGetValue(key, out T result);
            if (isCached)
                return result;

            result = getFunction();
            Set(key, result, options);

            return result;
        }


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            TimeSpan absoluteExpirationRelativeToNow)
            => await GetOrSetAsync(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, DistributedCacheEntryOptions options)
        {
            var result = await GetValueAsync<T>(key);
            if (result != null)
                return result;

            result = await getFunction();
            await SetAsync(key, result, options);

            return result;
        }


        public void Remove(string key)
        {
            try
            {
                _distributedCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;
            }

            _logger.Remove(key);
        }


        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;
            }

            _logger.Remove(key);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
            => SetInternal(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options) 
            => SetInternal(key, value, options);


        public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
            => await SetInternalAsync(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow });


        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) 
            => await SetInternalAsync(key, value, options);


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            var bytes = GetFromCache(key);
            if (bytes is null)
            {
                _logger.Miss(key);
                return false;
            }

            value = MessagePackSerializer.Deserialize<T>(bytes);
            if (!RefreshCache(key))
                return false;

            _logger.Hit(key);
            return true;
        }


        private byte[] GetFromCache(string key)
        {
            try
            {
                return _distributedCache.Get(key);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;

                return null;
            }
        }


        private async Task<byte[]> GetFromCacheAsync(string key)
        {
            try
            {
                return await _distributedCache.GetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;

                return null;
            }
        }


        private bool RefreshCache(string key)
        {
            try
            {
                _distributedCache.Refresh(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;

                return false;
            }
        }


        private async Task<bool> RefreshCacheAsync(string key)
        {
            try
            {
                await _distributedCache.RefreshAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;

                return false;
            }
        }


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = MessagePackSerializer.Serialize(value);
            try
            {
                _distributedCache.Set(key, bytes, options);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;
            }
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = MessagePackSerializer.Serialize(value);
            try
            {
                await _distributedCache.SetAsync(key, bytes, options);
            }
            catch (Exception ex)
            {
                _logger.NetworkError(ex);
                if (!_options.SuppressNetworkExceptions)
                    throw;
            }
        }


        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedFlow> _logger;
        private readonly FlowOptions _options;
    }
}
