using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CacheFlowTests;

public class ExecutorThunderingHerdTests
{
    [Fact]
    public async Task TryExecuteAsync_WithSameKey_ShouldInvokeFunctionOnlyOnce()
    {
        var loggerMock = new Mock<ILogger>();
        var options = new FlowOptions { ThunderingHerdProtectionTimeout = TimeSpan.FromSeconds(10) };
        var executor = new Executor(loggerMock.Object, options);
        
        const string cacheKey = "test-key";
        var counter = 0;

        var tasks = new Task<int>[10];
        for (var i = 0; i < tasks.Length; i++)
            tasks[i] = executor.TryExecuteAsync(cacheKey, TestFunction).AsTask();
        
        var results = await Task.WhenAll(tasks);
        
        Assert.Equal(1, counter);
        foreach (var result in results)
            Assert.Equal(42, result);


        async ValueTask<int> TestFunction()
        {
            Interlocked.Increment(ref counter);
            await Task.Delay(100);

            return 42;
        }
    }

    
    [Fact]
    public async Task TryExecuteAsync_WithDifferentKeys_ShouldInvokeFunctionMultipleTimes()
    {
        var loggerMock = new Mock<ILogger>();
        var options = new FlowOptions { ThunderingHerdProtectionTimeout = TimeSpan.FromSeconds(10) };
        var executor = new Executor(loggerMock.Object, options);
        
        var counter = 0;
        
        var tasks = new Task<int>[10];
        for (var i = 0; i < tasks.Length; i++)
        {
            var key = $"test-key-{i}";
            tasks[i] = executor.TryExecuteAsync(key, TestFunction).AsTask();
        }
        
        await Task.WhenAll(tasks);
        
        Assert.Equal(10, counter);


        async ValueTask<int> TestFunction()
        {
            Interlocked.Increment(ref counter);
            await Task.Delay(50);

            return 42;
        }
    }

    
    [Fact]
    public async Task TryExecuteAsync_WithSameKey_WhenFirstCallFails_ShouldPropagateException()
    {
        var loggerMock = new Mock<ILogger>();
        var options = new FlowOptions { 
            ThunderingHerdProtectionTimeout = TimeSpan.FromSeconds(10),
            SuppressCacheExceptions = false
        };
        var executor = new Executor(loggerMock.Object, options);
        
        const string cacheKey = "test-key";
        var counter = 0;
        
        var tasks = new Task[5];
        for (var i = 0; i < tasks.Length; i++)
            tasks[i] = AssertThrowsAsync<InvalidOperationException>(() => executor.TryExecuteAsync(cacheKey, TestFunction).AsTask());
        
        await Task.WhenAll(tasks);
        
        Assert.Equal(1, counter);


        async ValueTask<int> TestFunction()
        {
            Interlocked.Increment(ref counter);
            await Task.Delay(50);

            throw new InvalidOperationException("Test exception");
        }
    }

    
    [Fact]
    public async Task TryExecuteAsync_WithTimeout_ShouldInvokeFactoryAgainAfterTimeout()
    {
        var loggerMock = new Mock<ILogger>();
        var options = new FlowOptions { ThunderingHerdProtectionTimeout = TimeSpan.FromMilliseconds(100) };
        var executor = new Executor(loggerMock.Object, options);
    
        const string cacheKey = "test-key";
        var counter = 0;
        var firstCallStarted = new TaskCompletionSource<bool>();
        var releaseFirstCall = new TaskCompletionSource<bool>();
        var secondCallReceived = false;

        var hangingTask = executor.TryExecuteAsync(cacheKey, async () => 
        {
            var currentCount = Interlocked.Increment(ref counter);
            firstCallStarted.SetResult(true);
            await releaseFirstCall.Task;
            return 42;
        }).AsTask();
    
        await firstCallStarted.Task;
    
        await Task.Delay(200);
    
        var timeoutTask = executor.TryExecuteAsync(cacheKey, async () => 
        {
            var currentCount = Interlocked.Increment(ref counter);
            secondCallReceived = true;
            return 43;
        }).AsTask();
    
        releaseFirstCall.SetResult(true);
    
        var hangingResult = await hangingTask;
        var timeoutResult = await timeoutTask;
    
        Assert.Equal(2, counter);
        Assert.True(secondCallReceived);
        Assert.Equal(42, hangingResult);
        Assert.Equal(43, timeoutResult);
    }
    

    [Fact]
    public async Task TryExecuteAsync_WithMultipleKeys_ShouldMaintainConcurrentDictionary()
    {
        var loggerMock = new Mock<ILogger>();
        var options = new FlowOptions { ThunderingHerdProtectionTimeout = TimeSpan.FromMilliseconds(500) };
        var executor = new Executor(loggerMock.Object, options);
        
        var results = new ConcurrentDictionary<string, int>();
        var counters = new ConcurrentDictionary<string, int>();
        
        const int keyCount = 10;
        const int tasksPerKey = 5;
        var allTasks = new List<Task>();
        
        for (int i = 0; i < keyCount; i++)
        {
            var key = $"key-{i}";
            for (int j = 0; j < tasksPerKey; j++)
            {
                allTasks.Add(Task.Run(async () => 
                {
                    var result = await executor.TryExecuteAsync(key, () => TestFunction(key));
                    results[key] = result;
                }));
            }
        }
        
        await Task.WhenAll(allTasks);
        
        Assert.Equal(keyCount, counters.Count);
        Assert.Equal(keyCount, results.Count);
        foreach (var counter in counters)
            Assert.Equal(1, counter.Value);


        async ValueTask<int> TestFunction(string key)
        {
            counters.AddOrUpdate(key, 1, (_, count) => count + 1);
            await Task.Delay(100);

            return key.GetHashCode();
        }
    }


    private async Task AssertThrowsAsync<TException>(Func<Task> func) where TException : Exception
    {
        var exception = await Assert.ThrowsAsync<TException>(func);
        Assert.IsType<TException>(exception);
    }
}