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
                _logger?.LogNoOptionsProvided();
                _options = new FlowOptions();
            }
            else
            {
                _options = options.Value;
            }

            if (_options.UseBinarySerialization)
                CompositeResolver.RegisterAndSetAsDefault(ImmutableCollectionResolver.Instance,
                    StandardResolver.Instance);

            _nextQueryTime = DateTime.UtcNow;
        }


        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return default;
            }

            var cached = await GetFromCacheAsync(key, cancellationToken);
            if (cached is null)
            {
                _logger?.LogMiss(key);
                return default;
            }

            var value = Deserialize<T>(cached, _options.UseBinarySerialization);
            _logger?.LogHit(key);
            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return getFunction();
            }

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
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return await getFunction();
            }

            var result = await GetAsync<T>(key, cancellationToken);
            if (result != null)
                return result;

            result = await getFunction();
            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void Refresh(string key)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return;
            }

            TryExecute(() => _distributedCache.Refresh(key));
        }


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!IsOffline())
                return TryExecuteAsync(async () => await _distributedCache.RefreshAsync(key, cancellationToken));

            _logger?.LogSkipped(key);
            return default;

        }


        public void Remove(string key)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return;
            }

            TryExecute(() => _distributedCache.Remove(key));
            _logger?.LogRemoved(key);
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await TryExecuteAsync(async () => await _distributedCache.RemoveAsync(key, cancellationToken));
            _logger?.LogRemoved(key);
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
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return false;
            }

            var cached = GetFromCache(key);
            if (cached is null)
            {
                _logger?.LogMiss(key);
                return false;
            }

            value = Deserialize<T>(cached, _options.UseBinarySerialization);
            _logger?.LogHit(key);
            return true;
        }


        private static bool IsOffline()
        {
            if (!_isOffline)
                return false;

            if (DateTime.UtcNow <= _nextQueryTime)
                return true;

            _isOffline = false;
            return _isOffline;
        }


        private static T Deserialize<T>(object cached, bool isBinarySerialized)
        {
            return isBinarySerialized
                ? MessagePackSerializer.Deserialize<T>(cached as byte[])
                : JsonConvert.DeserializeObject<T>(cached as string);
        }


        private object GetFromCache(string key)
            => TryExecute(() =>
            {
                if (_options.UseBinarySerialization)
                    return _distributedCache.Get(key);

                return _distributedCache.GetString(key);
            });


        private Task<object> GetFromCacheAsync(string key, CancellationToken cancellationToken)
            => TryExecuteAsync(async () =>
            {
                if (_options.UseBinarySerialization)
                    return await _distributedCache.GetAsync(key, cancellationToken);

                return await _distributedCache.GetStringAsync(key, cancellationToken);
            });


        private static object Serialize<T>(T value, bool isBinarySerialized)
        {
            if (isBinarySerialized)
                return MessagePackSerializer.Serialize(value);

            return JsonConvert.SerializeObject(value);
        }


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return;
            }

            var serialized = Serialize(value, _options.UseBinarySerialization);
            TryExecute(() =>
            {
                if (_options.UseBinarySerialization)
                    _distributedCache.Set(key, serialized as byte[], options);
                else
                    _distributedCache.SetString(key, serialized as string, options);
            });
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken)
        {
            if (IsOffline())
            {
                _logger?.LogSkipped(key);
                return;
            }

            var serialized = Serialize(value, _options.UseBinarySerialization);
            await TryExecuteAsync(async () =>
            {
                if (_options.UseBinarySerialization)
                    await _distributedCache.SetAsync(key, serialized as byte[], options, cancellationToken);
                else
                    await _distributedCache.SetStringAsync(key, serialized as string, options, cancellationToken);
            });
        }


        private static void SetNextQueryTime(TimeSpan interval)
        {
            _nextQueryTime = DateTime.UtcNow.Add(interval);
            _isOffline = true;
        }


        private void TryExecute(Action action)
        {
            try
            {
                action();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogNetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            SetNextQueryTime(_options.NoRetryInterval);
        }


        private object TryExecute(Func<object> func)
        {
            try
            {
                return func();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogNetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            SetNextQueryTime(_options.NoRetryInterval);
            return null;
        }


        private async Task TryExecuteAsync(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogNetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            SetNextQueryTime(_options.NoRetryInterval);
        }


        private async Task<object> TryExecuteAsync(Func<Task<object>> func)
        {
            try
            {
                return await func();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogNetworkError(ex);
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            SetNextQueryTime(_options.NoRetryInterval);
            return null;
        }


        private readonly IDistributedCache _distributedCache;
        private static bool _isOffline;
        private readonly ILogger<DistributedFlow> _logger;
        private static DateTime _nextQueryTime;
        private readonly FlowOptions _options;
    }
}
