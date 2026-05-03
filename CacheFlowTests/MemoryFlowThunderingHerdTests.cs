using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CacheFlowTests;

public class MemoryFlowThunderingHerdTests
{
    [Fact]
    public async Task GetOrSet_WithConcurrentRequests_ShouldOnlyCallFactoryOnce()
    {
        // Arrange
        const int threadCount = 10;
        var cache = new MemoryFlow(new MemoryCache(new MemoryCacheOptions()));
        var factoryCallCount = 0;
        // Gate holds all threads until they are all ready, then releases them together
        var gate = new ManualResetEventSlim(false);

        string Factory()
        {
            Interlocked.Increment(ref factoryCallCount);
            Thread.Sleep(20);

            return "value";
        }

        // Act — all threads wait at the gate, then race to GetOrSet simultaneously
        var tasks = new List<Task<string>>();
        for (var i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                gate.Wait();
                return cache.GetOrSet("key", Factory, TimeSpan.FromMinutes(1));
            }));
        }

        gate.Set();
        var results = await Task.WhenAll(tasks);

        // Assert — factory must have been invoked exactly once
        Assert.Equal(1, factoryCallCount);
        Assert.All(results, r => Assert.Equal("value", r));
    }


    [Fact]
    public void GetOrSet_WhenFactoryThrows_ShouldSuppressAndReturnDefault()
    {
        // Default FlowOptions has SuppressCacheExceptions = true
        var cache = new MemoryFlow(new MemoryCache(new MemoryCacheOptions()));
        var attempt = 0;

        string Factory()
        {
            Interlocked.Increment(ref attempt);
            throw new InvalidOperationException("transient error");
        }

        // Act — exception is suppressed; default is returned
        var result = cache.GetOrSet("key", Factory, TimeSpan.FromMinutes(1));
        Assert.Null(result);
    }


    [Fact]
    public void GetOrSet_WhenFactoryThrows_ShouldEvictLazyAndAllowRetry()
    {
        var cache = new MemoryFlow(new MemoryCache(new MemoryCacheOptions()));
        var attempt = 0;

        string Factory()
        {
            if (Interlocked.Increment(ref attempt) == 1)
                throw new InvalidOperationException("transient error");
            return "recovered";
        }

        // First call: exception is suppressed, default returned
        var first = cache.GetOrSet("key", Factory, TimeSpan.FromMinutes(1));
        Assert.Null(first);

        // Second call: failed Lazy evicted, retry succeeds
        var second = cache.GetOrSet("key", Factory, TimeSpan.FromMinutes(1));
        Assert.Equal("recovered", second);
    }
}
