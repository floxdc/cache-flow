using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using FloxDc.CacheFlow.Logging;
using FloxDc.CacheFlow.Telemetry;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class MemoryFlow : IMemoryFlow
    {
        public MemoryFlow(IMemoryCache memoryCache, ILogger<MemoryFlow>? logger = default,
            IOptions<FlowOptions>? options = default)
        {
            Instance = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? new NullLogger<MemoryFlow>();
            _activitySource = ActivitySourceContainer.Instance;

            if (options is null)
            {
                _logger.LogNoOptionsProvided(nameof(MemoryFlow));
                Options = new FlowOptions();
            }
            else
            {
                Options = options.Value;
            }

            if (Options.DataLoggingLevel == DataLogLevel.Disabled)
                _logger = new NullLogger<MemoryFlow>();

            _executor = new Executor(_logger, Options.SuppressCacheExceptions, Options.DataLoggingLevel);
            _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);
        }


        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getValueFunction,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options)
        {
            if (TryGetValue(key, out T result))
                return result;

            var activity = _activitySource.GetStartedActivity("Value calculations");
            result = getValueFunction();
            _activitySource.StopStartedActivity(activity);
            
            Set(key, result, options);

            return result;
        }


        public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default) 
            => GetOrSetAsync(key, getValueFunction, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow}, cancellationToken);


        public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction,
            MemoryCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            if (TryGetValue(key, out T result))
                return result;

            var activity = _activitySource.GetStartedActivity("Async value calculations");
            result = await getValueFunction();
            _activitySource.StopStartedActivity(activity);
            
            Set(key, result, options);

            return result;
        }


        public void Remove(string key)
        {
            var activity = _activitySource.GetStartedActivity(nameof(Remove));
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            _executor.TryExecute(() => Instance.Remove(fullKey));

            if (Options.DataLoggingLevel == DataLogLevel.Sensitive)
                _logger.LogRemoved(nameof(MemoryFlow) + ":" + nameof(Remove), fullKey);
            else
                _logger.LogRemovedInsensitive(key);

            _activitySource.StopStartedActivity(activity, BuildArgs(CacheEvents.Remove, fullKey));
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
            => Set(key, value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        {
            var activity = _activitySource.GetStartedActivity(nameof(Set));
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            if (Utils.IsDefaultStruct(value))
            {
                if (Options.DataLoggingLevel == DataLogLevel.Sensitive)
                    _logger.LogNotSet(nameof(MemoryFlow) + ":" + nameof(Set), fullKey, value!);
                else
                    _logger.LogNotSetInsensitive(key);

                _activitySource.StopStartedActivity(activity, BuildArgs(CacheEvents.Skipped, fullKey));
                return;
            }

            _executor.TryExecute(() =>
            {
                using var entry = Instance.CreateEntry(fullKey);
                entry.SetOptions(options);
                entry.Value = value;
            });

            if (Options.DataLoggingLevel == DataLogLevel.Sensitive)
                _logger.LogSet(nameof(MemoryFlow) + ":" + nameof(Set), fullKey, value!);
            else
                _logger.LogSetInsensitive(key);

            _activitySource.StopStartedActivity(activity, BuildArgs(CacheEvents.Set, fullKey));
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            var activity = _activitySource.GetStartedActivity(nameof(TryGetValue));
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);

            bool isCached;
            (isCached, value) = _executor.TryExecute(() =>
            {
                var result = Instance.TryGetValue(fullKey, out T obj);
                return (result, obj);
            });

            if (!isCached)
            {
                if (Options.DataLoggingLevel == DataLogLevel.Sensitive)
                    _logger.LogMissed(nameof(MemoryFlow) + ":" + nameof(TryGetValue), fullKey);
                else
                    _logger.LogMissedInsensitive(key);

                _activitySource.StopStartedActivity(activity, BuildArgs(CacheEvents.Miss, fullKey));
                return false;
            }

            if (Options.DataLoggingLevel == DataLogLevel.Sensitive)
                _logger.LogHit(nameof(MemoryFlow) + ":" + nameof(TryGetValue), fullKey, value!);
            else
                _logger.LogHitInsensitive(key);
            
            _activitySource.StopStartedActivity(activity, BuildArgs(CacheEvents.Hit, fullKey));
            return true;
        }


        private static Dictionary<string, string> BuildArgs(CacheEvents @event, string key)
            => new()
            {
                { "event", @event.ToString() },
                { "key", key },
                { "service-type", nameof(MemoryFlow) }
            };


        public IMemoryCache Instance { get; }
        public FlowOptions Options { get; }


        private readonly ActivitySource _activitySource;
        private readonly Executor _executor;
        private readonly ILogger<MemoryFlow> _logger;
        private readonly string _prefix;
    }
}
