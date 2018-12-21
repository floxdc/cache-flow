using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
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
    public class DistributedFlow : IDistributedFlow
    {
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow> logger,
            IOptions<FlowOptions> options)
        {
            Instance = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
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

            _executor = new Executor(_logger, _options);
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

            if (TryGetValue(key, out T result))
                return result;

            result = getFunction();
            Set(key, result, options);

            return result;
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, getFunction,
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

            TryExecute(() => Instance.Refresh(key));
        }


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!IsOffline())
                return TryExecuteAsync(async () => await Instance.RefreshAsync(key, cancellationToken));

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

            TryExecute(() => Instance.Remove(key));
            _logger?.LogRemoved(key);
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await TryExecuteAsync(async () => await Instance.RemoveAsync(key, cancellationToken));
            _logger?.LogRemoved(key);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
            => SetInternal(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
            => SetInternal(key, value, options);


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => SetInternalAsync(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
            => SetInternalAsync(key, value, options, cancellationToken);


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
                    return Instance.Get(key);

                return Instance.GetString(key);
            });


        private Task<object> GetFromCacheAsync(string key, CancellationToken cancellationToken)
            => TryExecuteAsync(async () =>
            {
                if (_options.UseBinarySerialization)
                    return await Instance.GetAsync(key, cancellationToken);

                return await Instance.GetStringAsync(key, cancellationToken);
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
                    Instance.Set(key, serialized as byte[], options);
                else
                    Instance.SetString(key, serialized as string, options);
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
                    await Instance.SetAsync(key, serialized as byte[], options, cancellationToken);
                else
                    await Instance.SetStringAsync(key, serialized as string, options, cancellationToken);
            });

            _logger?.LogSet(key);
        }


        private static void SetNextQueryTime(TimeSpan interval)
        {
            _nextQueryTime = DateTime.UtcNow.Add(interval);
            _isOffline = true;
        }


        private void TryExecute(Action action)
        {
            if (_executor.TryExecute(action))
                return;

            SetNextQueryTime(_options.SkipRetryInterval);
        }


        private object TryExecute(Func<object> func)
        {
            var result = _executor.TryExecute(func);
            if(result is null)
                SetNextQueryTime(_options.SkipRetryInterval);

            return result;
        }


        private async Task TryExecuteAsync(Func<Task> func)
        {
            if (await _executor.TryExecuteAsync(func))
                return;

            SetNextQueryTime(_options.SkipRetryInterval);
        }


        private async Task<object> TryExecuteAsync(Func<Task<object>> func)
        {
            var result = await _executor.TryExecuteAsync(func);
            if (result is null)
                SetNextQueryTime(_options.SkipRetryInterval);

            return null;
        }


        public IDistributedCache Instance { get; }

        private readonly Executor _executor;
        private static bool _isOffline;
        private readonly ILogger<DistributedFlow> _logger;
        private static DateTime _nextQueryTime;
        private readonly FlowOptions _options;
    }
}
