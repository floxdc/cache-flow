using System;
using System.Threading;
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
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow> logger,
            IOptions<FlowOptions> options)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger;

            if (options is null)
            {
                _logger?.NoOptionsProvided();
                _options = new FlowOptions();
            }
            else
            {
                _options = options.Value;
            }

            if (_options.UseBinarySerialization)
                CompositeResolver.RegisterAndSetAsDefault(ImmutableCollectionResolver.Instance,
                    StandardResolver.Instance);
        }


        public async Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var bytes = await GetFromCacheAsync(key, cancellationToken);
            if (bytes is null)
            {
                _logger?.Miss(key);
                return default;
            }

            var value = MessagePackSerializer.Deserialize<T>(bytes);
            _logger?.Hit(key);
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
            TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => await GetOrSetAsync(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            var result = await GetValueAsync<T>(key, cancellationToken);
            if (result != null)
                return result;

            result = await getFunction();
            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void RefreshCache(string key)
        {
            try
            {
                _distributedCache.Refresh(key);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }
        }


        public async Task RefreshCacheAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _distributedCache.RefreshAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }
        }


        public void Remove(string key)
        {
            try
            {
                _distributedCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            _logger?.Remove(key);
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            _logger?.Remove(key);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
            => SetInternal(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
            => SetInternal(key, value, options);


        public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => await SetInternalAsync(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
            => await SetInternalAsync(key, value, options, cancellationToken);


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            var bytes = GetFromCache(key);
            if (bytes is null)
            {
                _logger?.Miss(key);
                return false;
            }

            value = MessagePackSerializer.Deserialize<T>(bytes);
            _logger?.Hit(key);
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
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;

                return null;
            }
        }


        private async Task<byte[]> GetFromCacheAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                return await _distributedCache.GetAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;

                return null;
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
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken)
        {
            var bytes = MessagePackSerializer.Serialize(value);
            try
            {
                await _distributedCache.SetAsync(key, bytes, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }
        }


        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedFlow> _logger;
        private readonly FlowOptions _options;
    }
}
