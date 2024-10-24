using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
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

namespace FloxDc.CacheFlow;

public class DistributedFlow : FlowBase, IDistributedFlow
{
    public DistributedFlow(IDistributedCache distributedCache, ISerializer serializer, ILogger<DistributedFlow>? logger = default,
        IOptions<FlowOptions>? options = default)
    {
        Instance = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

        _activitySource = ActivitySourceContainer.Instance;
        _logger = logger ?? new NullLogger<DistributedFlow>();
        _serializer = serializer;

        if (options is null)
        {
            _logger.LogNoOptionsProvided(nameof(DistributedFlow));
            Options = new FlowOptions();
        }
        else
        {
            Options = options.Value;
        }

        if (Options.DataLoggingLevel is DataLogLevel.Disabled)
            _logger = new NullLogger<DistributedFlow>();

        _executor = new Executor(_logger, Options.SuppressCacheExceptions, Options.DataLoggingLevel);
        _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(Options.CacheKeyPrefix, Options.CacheKeyDelimiter);

        _logSensitive = Options.DataLoggingLevel is DataLogLevel.Sensitive;
    }


    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(GetAsync), BuildTags(CacheEvent.Miss, fullKey));

        var cached = await _executor.TryExecuteAsync(async () => (await Instance.GetAsync(fullKey, cancellationToken)).AsMemory());
        if (cached.IsEmpty)
        {
            _logger.LogMissed(BuildTarget(nameof(GetAsync)), fullKey, _logSensitive);
            return default;
        }

        var value = DeserializeAndDecode<T>(_serializer, cached);

        _logger.LogHit(BuildTarget(nameof(GetAsync)), fullKey, value!, _logSensitive);
        activity.SetEvent(CacheEvent.Hit);

        return value;
    }


    public T? GetOrSet<T>(string key, Func<T> getFunction, in TimeSpan absoluteExpirationRelativeToNow)
        => GetOrSet(key, getFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


    public T? GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options)
    {
        if (TryGetValue(key, out T? result))
            return result;

        using (var _ = _activitySource.CreateStartedActivity("Value calculations"))
            result = getFunction();

        Set(key, result, options);
        return result;
    }


    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
        => GetOrSetAsync(key, getFunction, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow },
            cancellationToken);


    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<T>(key, cancellationToken);
        if (result is not null && !result.Equals(default(T)))
            return result;

        using (var _ = _activitySource.CreateStartedActivity("Async value calculations"))
            result = await getFunction();

        await SetAsync(key, result, options, cancellationToken);
        return result;
    }


    public void Refresh(string key)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(Refresh), BuildTags(CacheEvent.Set, fullKey));
        
        TryExecute(() => Instance.Refresh(fullKey));
    }


    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(RefreshAsync), BuildTags(CacheEvent.Set, fullKey));
        
        await TryExecuteAsync(async () => await Instance.RefreshAsync(fullKey, cancellationToken));
    }


    public void Remove(string key)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(Remove), BuildTags(CacheEvent.Remove, fullKey));
        
        TryExecute(() => Instance.Remove(fullKey));

        _logger.LogRemoved(BuildTarget(nameof(Remove)), fullKey, _logSensitive);
    }


    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(RemoveAsync), BuildTags(CacheEvent.Remove, fullKey));
        
        await TryExecuteAsync(async () => await Instance.RemoveAsync(fullKey, cancellationToken));

        _logger.LogRemoved(BuildTarget(nameof(RemoveAsync)), fullKey, _logSensitive);
    }


    public void Set<T>(string key, T value, in TimeSpan absoluteExpirationRelativeToNow)
        => SetInternal(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow });


    public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
        => SetInternal(key, value, options);


    public Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
        => SetInternalAsync(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow },
            cancellationToken);


    public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        => SetInternalAsync(key, value, options, cancellationToken);


    public bool TryGetValue<T>(string key, out T? value)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(TryGetValue), BuildTags(CacheEvent.Miss, fullKey));

        value = default;
        var cached = _executor.TryExecute(() => Instance.Get(fullKey).AsMemory());
        if (cached.IsEmpty)
        {
            _logger.LogMissed(BuildTarget(nameof(TryGetValue)), fullKey, _logSensitive);
            return false;
        }

        value = DeserializeAndDecode<T>(_serializer, cached);

        _logger.LogHit(BuildTarget(nameof(TryGetValue)), fullKey, value!, _logSensitive);
        activity.SetEvent(CacheEvent.Hit);

        return true;
    }


    private static string BuildTarget(string methodName) 
        => BuildTarget(nameof(DistributedFlow), methodName);


    private bool CanSet<T>(string key, T value, Activity? activity)
    {
        if (!Utils.IsDefaultStruct(value))
            return true;

        _logger.LogNotSet(BuildTarget(nameof(CanSet)), key, value!, _logSensitive);
        activity.SetEvent(CacheEvent.Skipped);

        return false;
    }


    private static T DeserializeAndDecode<T>(ISerializer serializer, in ReadOnlyMemory<byte> value)
        => serializer.IsBinarySerializer
            ? serializer.Deserialize<T>(value)
            : serializer.Deserialize<T>(Encoding.UTF8.GetString(value.Span));


    private void SetInternal<T>(string key, T value, DistributedCacheEntryOptions options)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(Set), BuildTags(CacheEvent.Set, fullKey));
        
        if (!CanSet(fullKey, value, activity))
            return;

        TryExecute(() =>
        {
            var encoded = _serializer.Serialize(value);
            Instance.Set(fullKey, encoded, options);
        });

        _logger.LogSet(BuildTarget(nameof(SetInternal)), fullKey, value!, _logSensitive);
    }


    private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
    {
        var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
        using var activity = _activitySource.CreateStartedActivity(nameof(SetAsync), BuildTags(CacheEvent.Set, fullKey));
        
        if (!CanSet(fullKey, value, activity))
            return;

        await TryExecuteAsync(async () =>
        {
            var encoded = _serializer.Serialize(value);
            await Instance.SetAsync(fullKey, encoded, options, cancellationToken);
        });

        _logger.LogSet(BuildTarget(nameof(SetInternalAsync)), fullKey, value!, _logSensitive);
    }


    private void TryExecute(Action action) 
        => _executor.TryExecute(action);


    private Task TryExecuteAsync(Func<Task> func) 
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