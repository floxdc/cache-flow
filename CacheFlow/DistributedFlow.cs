using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DistributedFlow : IDistributedFlow
    {
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow> logger = default,
            IOptions<FlowOptions> options = default, ISerializer serializer = default)
        {
            Instance = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? new NullLogger<DistributedFlow>();
            _serializer = serializer ?? new BinarySerializer();

            if (options is null)
            {
                _logger.LogNoOptionsProvided();
                Options = new FlowOptions();
            }
            else
            {
                Options = options.Value;
            }

            _nextQueryTime = DateTime.UtcNow;
            _executor = new Executor(_logger, Options.SuppressCacheExceptions);
            _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);
        }


        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (IsOffline())
            {
                _logger.LogSkipped(key);
                return default;
            }

            var cached = await GetInternalAsync(key, cancellationToken);
            if (cached is null)
            {
                _logger.LogMissed(key);
                return default;
            }

            var value = DeserializeAndDecode<T>(_serializer, cached);
            _logger.LogHitted(key);
            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
        {
            if (IsOffline())
            {
                _logger.LogSkipped(key);
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
                _logger.LogSkipped(key);
                return await getFunction();
            }

            var result = await GetAsync<T>(key, cancellationToken);
            if (result != null && !result.Equals(default(T)))
                return result;

            result = await getFunction();
            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void Refresh(string key)
        {
            if (IsOffline())
            {
                _logger.LogSkipped(key);
                return;
            }

            TryExecute(() =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Refresh(fullKey);
            });
        }


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!IsOffline())
                return TryExecuteAsync(async () =>
                {
                    var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                    await Instance.RefreshAsync(fullKey, cancellationToken);
                });

            _logger.LogSkipped(key);
            return default;
        }


        public void Remove(string key)
        {
            if (IsOffline())
            {
                _logger.LogSkipped(key);
                return;
            }

            
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            TryExecute(() => Instance.Remove(fullKey));
            _logger.LogRemoved(key);
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await TryExecuteAsync(async () => 
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.RemoveAsync(fullKey, cancellationToken);
            });

            _logger.LogRemoved(key);
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
                _logger.LogSkipped(key);
                return false;
            }

            var cached = GetInternal(key);
            if (cached is null)
            {
                _logger.LogMissed(key);
                return false;
            }

            value = DeserializeAndDecode<T>(_serializer, cached);
            _logger.LogHitted(key);
            return true;
        }


        private static bool CanSet<T>(ILogger<DistributedFlow> logger, string key, T value)
        {
            if (IsOffline())
            {
                logger.LogSkipped(key);
                return false;
            }

            if (!Utils.IsDefaultStruct(value))
                return true;

            logger.LogNotSetted(key);
            return false;
        }


        private static T DeserializeAndDecode<T>(ISerializer serializer, byte[] value)
            => serializer.IsBinarySerializer
                ? serializer.Deserialize<T>(value)
                : serializer.Deserialize<T>(Encoding.UTF8.GetString(value, 0, value.Length));


        private static bool IsOffline()
        {
            if (!_isOffline)
                return false;

            if (DateTime.UtcNow <= _nextQueryTime)
                return true;

            _isOffline = false;
            return _isOffline;
        }


        private byte[] GetInternal(string key)
            => TryExecute(() =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                return Instance.Get(fullKey);
            });


        private Task<byte[]> GetInternalAsync(string key, CancellationToken cancellationToken)
            => TryExecuteAsync(async () =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                return await Instance.GetAsync(fullKey, cancellationToken);
            });


        private static byte[] SerializeAndEncode<T>(ISerializer serializer, T value)
        {
            var serialized = serializer.Serialize(value);
            return serializer.IsBinarySerializer
                ? serialized as byte[]
                : Encoding.UTF8.GetBytes(serialized as string);
        }


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            if (!CanSet(_logger, key, value))
                return;

            TryExecute(() =>
            {
                var encoded = SerializeAndEncode(_serializer, value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Set(fullKey, encoded, options);
            });

            _logger.LogSetted(key);
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        {
            if (!CanSet(_logger, key, value))
                return;

            await TryExecuteAsync(async () =>
            {
                var encoded = SerializeAndEncode(_serializer, value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.SetAsync(fullKey, encoded, options, cancellationToken);
            });

            _logger.LogSetted(key);
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

            SetNextQueryTime(Options.SkipRetryInterval);
        }


        private byte[] TryExecute(Func<byte[]> func)
        {
            var result = _executor.TryExecute(func);
            if(result is null || result.Equals(default))
                SetNextQueryTime(Options.SkipRetryInterval);

            return result;
        }


        private async Task TryExecuteAsync(Func<Task> func)
        {
            if (await _executor.TryExecuteAsync(func))
                return;

            SetNextQueryTime(Options.SkipRetryInterval);
        }


        private async Task<byte[]> TryExecuteAsync(Func<Task<byte[]>> func)
        {
            var result = await _executor.TryExecuteAsync(func);
            if (result is null || result.Equals(default))
                SetNextQueryTime(Options.SkipRetryInterval);

            return result;
        }


        public IDistributedCache Instance { get; }

        public FlowOptions Options { get; }


        private readonly Executor _executor;
        private static bool _isOffline;
        private readonly ILogger<DistributedFlow> _logger;
        private static DateTime _nextQueryTime;
        private readonly string _prefix;
        private readonly ISerializer _serializer;
    }
}
