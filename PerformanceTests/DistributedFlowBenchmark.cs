using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace PerformanceTests;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 50)]
public class DistributedFlowBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        var distributedCache = new MemoryDistributedCache();
        var options = Options.Create(new FlowOptions
        {
            CacheKeyPrefix = "test",
            CacheKeyDelimiter = ":",
            DataLoggingLevel = DataLogLevel.Normal,
            SuppressCacheExceptions = true
        });

        var serializerOptions = Options.Create(new JsonSerializerOptions());
        var serializer = new TextJsonSerializer(serializerOptions);
        var logger = new NullLogger<DistributedFlow>();

        _distributedFlow = new DistributedFlow(distributedCache, serializer, logger, options);
    }

    [Benchmark]
    public void GetOrSet()
    {
        _distributedFlow.GetOrSet(Key, () => Value, _cacheEntryOptions);
    }

    [Benchmark]
    public async Task GetOrSetAsync()
    {
        await _distributedFlow.GetOrSetAsync(Key, () => Task.FromResult(Value), _cacheEntryOptions);
    }

    //[Benchmark]
    //public async Task GetOrSetValueTaskAsync()
    //{
    //    await _distributedFlow.GetOrSetAsync(Key, () => ValueTask.FromResult(Value), _cacheEntryOptions);
    //}

    [Benchmark]
    public void Remove()
    {
        _distributedFlow.Remove(Key);
    }

    [Benchmark]
    public void Set()
    {
        _distributedFlow.Set(Key, Value, _cacheEntryOptions);
    }

    [Benchmark]
    public void TryGetValue()
    {
        _distributedFlow.TryGetValue(Key, out string _);
    }

    private const string Key = "testKey";
    private const string Value = "testValue";
    private readonly DistributedCacheEntryOptions _cacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private DistributedFlow _distributedFlow;
}
