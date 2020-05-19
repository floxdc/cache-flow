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
                _logger.LogNoOptionsProvided(nameof(DistributedFlow));
                Options = new FlowOptions();
            }
            else
            {
                Options = options.Value;
            }

            _executor = new Executor(_logger, Options.SuppressCacheExceptions, Options.EnableSensitiveDataLogging);
            _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);
        }


        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cached = await GetInternalAsync(key, cancellationToken);
            if (cached.IsEmpty)
            {
                if (Options.EnableSensitiveDataLogging)
                    _logger.LogMissed(nameof(DistributedFlow) + ":" + nameof(GetAsync), key);
                else
                    _logger.LogMissedInsensitive(key);

                return default;
            }

            var value = DeserializeAndDecode<T>(_serializer, cached);
            if (Options.EnableSensitiveDataLogging)
                _logger.LogHit(nameof(DistributedFlow) + ":" + nameof(GetAsync), key, value);
            else
                _logger.LogHitInsensitive(key);

            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
        {
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
            var result = await GetAsync<T>(key, cancellationToken);
            if (result != null && !result.Equals(default(T)))
                return result;

            result = await getFunction();
            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void Refresh(string key)
            => TryExecute(() =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Refresh(fullKey);
            });


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
            => TryExecuteAsync(async () =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.RefreshAsync(fullKey, cancellationToken);
            });


        public void Remove(string key)
        {
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            TryExecute(() => Instance.Remove(fullKey));

            if (Options.EnableSensitiveDataLogging)
                _logger.LogRemoved(nameof(DistributedFlow) + ":" + nameof(Remove), key);
            else
                _logger.LogRemovedInsensitive(key);
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await TryExecuteAsync(async () => 
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.RemoveAsync(fullKey, cancellationToken);
            });

            if (Options.EnableSensitiveDataLogging)
                _logger.LogRemoved(nameof(DistributedFlow) + ":" + nameof(RemoveAsync), key);
            else
                _logger.LogRemovedInsensitive(key);
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
            
            var cached = GetInternal(key);
            if (cached.IsEmpty)
            {
                if (Options.EnableSensitiveDataLogging)
                    _logger.LogMissed(nameof(DistributedFlow) + ":" + nameof(TryGetValue), key);
                else
                    _logger.LogMissedInsensitive(key);

                return false;
            }

            value = DeserializeAndDecode<T>(_serializer, cached);

            if (Options.EnableSensitiveDataLogging)
                _logger.LogHit(nameof(DistributedFlow) + ":" + nameof(TryGetValue), key, value);
            else
                _logger.LogHitInsensitive(key);

            return true;
        }


        private bool CanSet<T>(string key, T value)
        {
            if (!Utils.IsDefaultStruct(value))
                return true;

            if (Options.EnableSensitiveDataLogging)
                _logger.LogNotSet(nameof(DistributedFlow) + ":" + nameof(CanSet), key, value);
            else
                _logger.LogNotSetInsensitive(key);

            return false;
        }


        private static T DeserializeAndDecode<T>(ISerializer serializer, in ReadOnlyMemory<byte> value)
            => serializer.IsBinarySerializer
                ? serializer.Deserialize<T>(value)
                : serializer.Deserialize<T>(Encoding.UTF8.GetString(value.Span));


        private ReadOnlyMemory<byte> GetInternal(string key)
            => TryExecute(() =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                return Instance.Get(fullKey).AsMemory();
            });


        private Task<ReadOnlyMemory<byte>> GetInternalAsync(string key, CancellationToken cancellationToken)
            => TryExecuteAsync(async () =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                return (await Instance.GetAsync(fullKey, cancellationToken)).AsMemory();
            });


        /*private static byte[] SerializeAndEncode<T>(ISerializer serializer, T value)
        {
            var serialized = serializer.Serialize(value);
            return serializer.IsBinarySerializer
                ? serialized as byte[]
                : Encoding.UTF8.GetBytes(serialized as string);
        }*/


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            if (!CanSet(key, value))
                return;

            TryExecute(() =>
            {
                var encoded = _serializer.Serialize(value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Set(fullKey, encoded, options);
            });

            if (Options.EnableSensitiveDataLogging)
                _logger.LogSet(nameof(DistributedFlow) + ":" + nameof(SetInternal), key, value);
            else
                _logger.LogSetInsensitive(key);
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        {
            if (!CanSet(key, value))
                return;

            await TryExecuteAsync(async () =>
            {
                var encoded = _serializer.Serialize(value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.SetAsync(fullKey, encoded, options, cancellationToken);
            });

            if (Options.EnableSensitiveDataLogging)
                _logger.LogSet(nameof(DistributedFlow) + ":" + nameof(SetInternalAsync), key, value);
            else
                _logger.LogSetInsensitive(key);
        }


        private void TryExecute(Action action) 
            => _executor.TryExecute(action);


        private ReadOnlyMemory<byte> TryExecute(Func<ReadOnlyMemory<byte>> func) 
            => _executor.TryExecute(func);


        private Task TryExecuteAsync(Func<Task> func) 
            => _executor.TryExecuteAsync(func);


        private Task<ReadOnlyMemory<byte>> TryExecuteAsync(Func<Task<ReadOnlyMemory<byte>>> func) 
            => _executor.TryExecuteAsync(func);


        public IDistributedCache Instance { get; }

        public FlowOptions Options { get; }


        private readonly Executor _executor;
        private readonly ILogger<DistributedFlow> _logger;
        private readonly string _prefix;
        private readonly ISerializer _serializer;
    }
}
