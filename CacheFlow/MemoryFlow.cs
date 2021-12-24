using System;
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

namespace FloxDc.CacheFlow;

public class MemoryFlow : FlowBase, IMemoryFlow
{
    public MemoryFlow(IMemoryCache memoryCache, ILogger<MemoryFlow>? logger = default,
        IOptions<FlowOptions>? options = default)
    {
        _activitySource = ActivitySourceContainer.Instance;
        Instance = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? new NullLogger<MemoryFlow>();

        if (options is null)
        {
            _logger.LogNoOptionsProvided(nameof(MemoryFlow));
            Options = new FlowOptions();
        }
        else
        {
            Options = options.Value;
        }

        if (Options.DataLoggingLevel is DataLogLevel.Disabled)
            _logger = new NullLogger<MemoryFlow>();

        _executor = new Executor(_logger, Options.SuppressCacheExceptions, Options.DataLoggingLevel);
        _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);

        _logSensitive = Options.DataLoggingLevel is DataLogLevel.Sensitive;
    }


    public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
        => GetOrSet(key, getValueFunction, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow });


    public T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options)
    {
        if (TryGetValue(key, out T result))
            return result;

        using (var _ = _activitySource.CreateStartedActivity("Value calculations"))
            result = getValueFunction();

        Set(key, result, options);
        return result;
    }


    public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
        => GetOrSetAsync(key, getValueFunction, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow },
            cancellationToken);


    public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetValue(key, out T result))
            return result;

        using (var _ = _activitySource.CreateStartedActivity("Async value calculations"))
            result = await getValueFunction();

        Set(key, result, options);
        return result;
    }


    public void Remove(string key)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(Remove), BuildTags(CacheEvents.Remove, fullKey));
        
        _executor.TryExecute(() => Instance.Remove(fullKey));

        _logger.LogRemoved(BuildTarget(nameof(Remove)), key, _logSensitive);
    }


    public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
        => Set(key, value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


    public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(Set), BuildTags(CacheEvents.Skipped, fullKey));
        if (Utils.IsDefaultStruct(value))
        {
            _logger.LogNotSet(BuildTarget(nameof(Set)), fullKey, value!, _logSensitive);
            return;
        }

        _executor.TryExecute(() =>
        {
            using var entry = Instance.CreateEntry(fullKey);
            entry.SetOptions(options);
            entry.Value = value;
        });

        _logger.LogSet(BuildTarget(nameof(Set)), fullKey, value!, _logSensitive);
        activity.SetEvent(CacheEvents.Set);
    }


    public bool TryGetValue<T>(string key, out T value)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(TryGetValue), BuildTags(CacheEvents.Miss, fullKey));

        bool isCached;
        (isCached, value) = _executor.TryExecute(() =>
        {
            var result = Instance.TryGetValue(fullKey, out T obj);
            return (result, obj);
        });

        if (!isCached)
        {
            _logger.LogMissed(BuildTarget(nameof(TryGetValue)), fullKey, _logSensitive);
            return false;
        }

        _logger.LogHit(BuildTarget(nameof(TryGetValue)), fullKey, value!, _logSensitive);
        activity.SetEvent(CacheEvents.Hit);

        return true;
    }


    private static string BuildTarget(string methodName) 
        => BuildTarget(nameof(DistributedFlow), methodName);


    public IMemoryCache Instance { get; }
    public FlowOptions Options { get; }


    private readonly ActivitySource _activitySource;
    private readonly Executor _executor;
    private readonly ILogger<MemoryFlow> _logger;
    private readonly bool _logSensitive;
    private readonly string _prefix;
}