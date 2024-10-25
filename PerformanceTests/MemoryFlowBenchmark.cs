using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, iterationCount: 100)]
public class MemoryFlowBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        var memoryCache = new MemoryCache();
        var options = Options.Create(new FlowOptions
        {
            CacheKeyPrefix = "test",
            CacheKeyDelimiter = ":",
            DataLoggingLevel = DataLogLevel.Normal,
            SuppressCacheExceptions = true
        });

        _memoryFlow = new MemoryFlow(memoryCache, new NullLogger<MemoryFlow>(), options);
    }

    [Benchmark]
    public void GetOrSet()
    {
        _memoryFlow.GetOrSet(Key, () => Value, _cacheEntryOptions);
    }


    [Benchmark]
    public async Task GetOrSetAsync()
    {
        await _memoryFlow.GetOrSetAsync(Key, () => Task.FromResult(Value), _cacheEntryOptions);
    }


    [Benchmark]
    public void Remove()
    {
        _memoryFlow.Remove(Key);
    }


    [Benchmark]
    public void Set()
    {
        _memoryFlow.Set(Key, Value, _cacheEntryOptions);
    }


    [Benchmark]
    public void TryGetValue()
    {
        _memoryFlow.TryGetValue(Key, out string value);
    }

    
    private const string Key = "testKey";
    private const string Value = "testValue";
    private MemoryCacheEntryOptions _cacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };
    
    private MemoryFlow _memoryFlow;
}
