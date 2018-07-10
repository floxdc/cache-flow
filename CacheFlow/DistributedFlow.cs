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
using Newtonsoft.Json;

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


        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cached = await GetFromCacheAsync(key, cancellationToken);
            if (cached is null)
            {
                _logger?.Miss(key);
                return default;
            }

            var value = Deserialize<T>(cached);
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
            var result = await GetAsync<T>(key, cancellationToken);
            if (result != null)
                return result;

            result = await getFunction();
            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void Refresh(string key)
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


        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
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

            var cached = GetFromCache(key);
            if (cached is null)
            {
                _logger?.Miss(key);
                return false;
            }

            value = Deserialize<T>(cached);
            _logger?.Hit(key);
            return true;
        }


        private T Deserialize<T>(object cached)
        {
            return _options.UseBinarySerialization
                ? MessagePackSerializer.Deserialize<T>(cached as byte[])
                : JsonConvert.DeserializeObject<T>(cached as string);
        }


        private object GetFromCache(string key)
        {
            try
            {
                if (_options.UseBinarySerialization)
                    return _distributedCache.Get(key);

                return _distributedCache.GetString(key);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;

                return null;
            }
        }


        private async Task<object> GetFromCacheAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                if (_options.UseBinarySerialization)
                    return await _distributedCache.GetAsync(key, cancellationToken);

                return await _distributedCache.GetStringAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.NetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;

                return null;
            }
        }


        private object Serialize<T>(T value)
        {
            if (_options.UseBinarySerialization)
                return MessagePackSerializer.Serialize(value);

            return JsonConvert.SerializeObject(value);
        }


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var serialized = Serialize(value);
            try
            {
                if (_options.UseBinarySerialization)
                    _distributedCache.Set(key, serialized as byte[], options);
                else
                    _distributedCache.SetString(key, serialized as string, options);
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
            var serialized = Serialize(value);
            try
            {
                if (_options.UseBinarySerialization)
                    await _distributedCache.SetAsync(key, serialized as byte [], options, cancellationToken);
                else
                    await _distributedCache.SetStringAsync(key, serialized as string, options, cancellationToken);
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
