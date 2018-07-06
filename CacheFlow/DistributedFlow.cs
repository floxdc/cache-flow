using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow
{
    public class DistributedFlow : ICacheFlow
    {
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;

            CompositeResolver.RegisterAndSetAsDefault(ImmutableCollectionResolver.Instance, StandardResolver.Instance);
        }


        public async Task<T> GetValueAsync<T>(string key)
        {
            var bytes = await GetFromCacheAsync(key);
            if (bytes is null)
            {
                LogMiss(key);
                return default;
            }

            var value = MessagePackSerializer.Deserialize<T>(bytes);
            if (!await RefreshCacheAsync(key))
                return default;

            LogHit(key);
            return value;
        }


        public T GetOrSet<T>(string key, TimeSpan absoluteExpirationRelativeToNow, Func<T> getFunction)
        {
            var isCached = TryGetValue(key, out T result);
            if (isCached)
                return result;

            result = getFunction();
            Set(key, result, absoluteExpirationRelativeToNow);

            return result;
        }


        public T GetOrSet<T>(string key, DistributedCacheEntryOptions options, Func<T> getFunction)
        {
            var isCached = TryGetValue(key, out T result);
            if (isCached)
                return result;

            result = getFunction();
            Set(key, result, options);

            return result;
        }


        public async Task<T> GetOrSetAsync<T>(string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<T>> getFunction)
        {
            var result = await GetValueAsync<T>(key);
            if (result != null)
                return result;

            result = await getFunction();
            await SetAsync(key, result, absoluteExpirationRelativeToNow);

            return result;
        }


        public async Task<T> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T>> getFunction)
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
                LogError(ex);
            }

            LogRemove(key);
        }


        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            LogRemove(key);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var options = new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow};
            SetInternal(key, value, options);
        }


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options) 
            => SetInternal(key, value, options);


        public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            await SetInternalAsync(key, value, options);
        }


        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options) 
            => await SetInternalAsync(key, value, options);


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            var bytes = GetFromCache(key);
            if (bytes is null)
            {
                LogMiss(key);
                return false;
            }

            value = MessagePackSerializer.Deserialize<T>(bytes);
            if (!RefreshCache(key))
                return false;

            LogHit(key);
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
                LogError(ex);
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
                LogError(ex);
                return null;
            }
        }


        private void LogError(Exception ex)
            => _logger.Log(LogLevel.Warning, LoggingExtensions.GetEventId(CacheEvents.AnErrorHasOccured), ex.Message, ex, LoggingExtensions.Formatter);


        private void LogHit(string key)
            => _logger.Log(LogLevel.Information, LoggingExtensions.GetEventId(CacheEvents.Hit), $"{CacheEvents.Hit}|{key}", null, LoggingExtensions.Formatter);


        private void LogMiss(string key)
            => _logger.Log(LogLevel.Information, LoggingExtensions.GetEventId(CacheEvents.Miss), $"{CacheEvents.Miss}|{key}", null, LoggingExtensions.Formatter);


        private void LogRemove(string key)
            => _logger.Log(LogLevel.Information, LoggingExtensions.GetEventId(CacheEvents.Remove), $"{CacheEvents.Remove}|{key}", null, LoggingExtensions.Formatter);


        private bool RefreshCache(string key)
        {
            try
            {
                _distributedCache.Refresh(key);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
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
                LogError(ex);
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
                LogError(ex);
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
                LogError(ex);
            }
        }


        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedFlow> _logger;
    }
}
