using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using FloxDc.CacheFlow.Logging;
using FloxDc.CacheFlow.Serialization;
using FloxDc.CacheFlow.Telemetry;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DistributedFlow : IDistributedFlow
    {
        public DistributedFlow(IDistributedCache distributedCache, ILogger<DistributedFlow>? logger = default,
            IOptions<FlowOptions>? options = default, ISerializer? serializer = default)
        {
            Instance = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? new NullLogger<DistributedFlow>();
            _activitySource = ActivitySourceContainer.Instance;
            _serializer = serializer ?? new TextJsonSerializer();

            if (options is null)
            {
                _logger.LogNoOptionsProvided(nameof(DistributedFlow));
                Options = new FlowOptions();
            }
            else
            {
                Options = options.Value;
            }

            if (Options.DataLoggingLevel == DataLogLevel.Disabled)
                _logger = new NullLogger<DistributedFlow>();

            _executor = new Executor(_logger, Options.SuppressCacheExceptions, Options.DataLoggingLevel);
            _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);

            _logSensitive = Options.DataLoggingLevel == DataLogLevel.Sensitive;
        }


        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var activity = _activitySource.GetStartedActivity(nameof(GetAsync));
            var cached = await GetInternalAsync(key, cancellationToken);
            if (cached.IsEmpty)
            {
                _logger.LogMissed(nameof(DistributedFlow) + ":" + nameof(GetAsync), key, _logSensitive);
                _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Miss, key));
                return default;
            }

            var value = DeserializeAndDecode<T>(_serializer, cached);
            _logger.LogHit(nameof(DistributedFlow) + ":" + nameof(GetAsync), key, value!, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Hit, key));
            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
        {
            if (TryGetValue(key, out T? result))
                return result!;

            var activity = _activitySource.GetStartedActivity("Value calculations");
            result = getFunction();
            _activitySource.StopStartedActivity(activity);

            Set(key, result, options);

            return result;
        }


        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            var result = await GetAsync<T>(key, cancellationToken);
            if (result != null && !result.Equals(default(T)))
                return result;
            
            var activity = _activitySource.GetStartedActivity("Async value calculations");
            result = await getFunction();
            _activitySource.StopStartedActivity(activity);

            await SetAsync(key, result, options, cancellationToken);

            return result;
        }


        public void Refresh(string key)
        {
            var activity = _activitySource.GetStartedActivity(nameof(Refresh));

            TryExecute(() =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Refresh(fullKey);
            });

            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Set, key));
        }


        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            var activity = _activitySource.GetStartedActivity(nameof(RefreshAsync));

            await TryExecuteAsync(async () =>
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.RefreshAsync(fullKey, cancellationToken);
            });

            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Set, key));
        }


        public void Remove(string key)
        {
            var activity = _activitySource.GetStartedActivity(nameof(Remove));
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            TryExecute(() => Instance.Remove(fullKey));

            _logger.LogRemoved(nameof(DistributedFlow) + ":" + nameof(Remove), key, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Remove, key));
        }


        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            var activity = _activitySource.GetStartedActivity(nameof(RemoveAsync));
            
            await TryExecuteAsync(async () => 
            {
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.RemoveAsync(fullKey, cancellationToken);
            });

            _logger.LogRemoved(nameof(DistributedFlow) + ":" + nameof(RemoveAsync), key, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Remove, key));
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


        public bool TryGetValue<T>(string key, out T? value)
        {
            var activity = _activitySource.GetStartedActivity(nameof(TryGetValue));

            value = default;
            var cached = GetInternal(key);
            if (cached.IsEmpty)
            {
                _logger.LogMissed(nameof(DistributedFlow) + ":" + nameof(TryGetValue), key, _logSensitive);
                _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Miss, key));
                return false;
            }

            value = DeserializeAndDecode<T>(_serializer, cached);

            _logger.LogHit(nameof(DistributedFlow) + ":" + nameof(TryGetValue), key, value!, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Hit, key));
            return true;
        }


        private static Dictionary<string, string> BuildTags(CacheEvents @event, string key)
            => new()
            {
                {"event", @event.ToString()},
                {"key", key},
                {"service-type", nameof(DistributedFlow)}
            };


        private bool CanSet<T>(string key, T value, Activity? activity)
        {
            if (!Utils.IsDefaultStruct(value))
                return true;

            _logger.LogNotSet(nameof(DistributedFlow) + ":" + nameof(CanSet), key, value!, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Skipped, key));
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


        private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var activity = _activitySource.GetStartedActivity(nameof(Set));
            if (!CanSet(key, value, activity))
                return;

            TryExecute(() =>
            {
                var encoded = _serializer.Serialize(value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                Instance.Set(fullKey, encoded, options);
            });

            _logger.LogSet(nameof(DistributedFlow) + ":" + nameof(SetInternal), key, value!, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Set, key));
        }


        private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        {
            var activity = _activitySource.GetStartedActivity(nameof(SetAsync));
            if (!CanSet(key, value, activity))
                return;

            await TryExecuteAsync(async () =>
            {
                var encoded = _serializer.Serialize(value);
                var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
                await Instance.SetAsync(fullKey, encoded, options, cancellationToken);
            });

            _logger.LogSet(nameof(DistributedFlow) + ":" + nameof(SetInternalAsync), key, value!, _logSensitive);
            _activitySource.StopStartedActivity(activity, BuildTags(CacheEvents.Set, key));
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


        private readonly ActivitySource _activitySource;
        private readonly Executor _executor;
        private readonly ILogger<DistributedFlow> _logger;
        private readonly bool _logSensitive;
        private readonly string _prefix;
        private readonly ISerializer _serializer;
    }
}
