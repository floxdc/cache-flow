using System;
using System.Threading.Tasks;
using DAW.CacheFlow;
using DAW.CacheFlow.Logging;
using DAW.LoggingExtensions;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CacheFlow
{
    public class RedisFlow : ICacheFlow
    {
        public RedisFlow(IDistributedCache distributedCache, ILogger<RedisFlow> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;

            CompositeResolver.RegisterAndSetAsDefault(ImmutableCollectionResolver.Instance, StandardResolver.Instance);
        }


        public async Task<T> GetValueAsync<T>(string key)
        {
            var bytes = await _distributedCache.GetAsync(key);
            if (bytes is null)
            {
                LogMiss(key);
                return default;
            }

            var value = MessagePackSerializer.Deserialize<T>(bytes);
            await _distributedCache.RefreshAsync(key);
            LogHit(key);
            
            return value;
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var options = new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow};
            SetInternal(key, value, options);
        }


        public void SetSliding<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
            SetInternal(key, value, options);
        }


        public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            await SetInternalAsync(key, value, options);
        }


        public async Task SetSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration)
        {
            var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };
            await SetInternalAsync(key, value, options);
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            var bytes = _distributedCache.Get(key);
            if (bytes is null)
            {
                LogMiss(key);
                return false;
            }

            value = MessagePackSerializer.Deserialize<T>(bytes);
            _distributedCache.Refresh(key);
            LogHit(key);

            return true;
        }


        private void LogHit(string key)
            => _logger.Log(LogLevel.Information, LoggingExtensions.GetEventId(CacheEvents.Hitted), $"Hit|{key}", null, LoggingExtensions.Formatter);


        private void LogMiss(string key)
            => _logger.Log(LogLevel.Information, LoggingExtensions.GetEventId(CacheEvents.Missed), $"Miss|{key}", null, LoggingExtensions.Formatter);


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = MessagePackSerializer.Serialize(value);
            _distributedCache.Set(key, bytes, options);
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var bytes = MessagePackSerializer.Serialize(value);
            await _distributedCache.SetAsync(key, bytes, options);
        }


        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisFlow> _logger;
    }
}
