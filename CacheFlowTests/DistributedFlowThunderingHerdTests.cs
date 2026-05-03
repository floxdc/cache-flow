using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CacheFlowTests;

public class DistributedFlowThunderingHerdTests
{
    [Fact]
    public async Task GetOrSetAsync_WithConcurrentRequests_ShouldOnlyCallFactoryOnce()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serializer = new TextJsonSerializer(Options.Create(new JsonSerializerOptions()));
        
        // Setup the cache to initially return null, simulating a cache miss
        distributedCacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null);

        // Capture the values that are set in the cache
        byte[] capturedCacheValue = null;
        distributedCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, options, token) => capturedCacheValue = value)
            .Returns(Task.CompletedTask);

        var options = new FlowOptions { ThunderingHerdProtectionTimeout = TimeSpan.FromSeconds(5) };
        var cache = new DistributedFlow(
            distributedCacheMock.Object, 
            serializer, 
            new Mock<ILogger<DistributedFlow>>().Object,
            Options.Create(options)
        );

        // Create a counter to track how many times the factory is called
        var factoryCalls = 0;
        
        // Create an expensive factory method
        async Task<string> FactoryMethod()
        {
            Interlocked.Increment(ref factoryCalls);
            // Simulate a slow operation
            await Task.Delay(500);
            return "cached-value";
        }

        // Act
        // Execute multiple concurrent GetOrSetAsync operations with the same key
        const int concurrentRequests = 10;
        var tasks = new Task<string>[concurrentRequests];
        
        for (var i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = cache.GetOrSetAsync(
                "test-key", 
                FactoryMethod, 
                TimeSpan.FromMinutes(5)
            );
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        // The factory should only be called once, even with 10 concurrent requests
        Assert.Equal(1, factoryCalls);
        
        // All tasks should get the same value
        foreach (var result in results)
        {
            Assert.Equal("cached-value", result);
        }
        
        // The cache should be called to set the value
        distributedCacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_WithFactoryTimeout_ShouldHandleErrorGracefully()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serializer = new TextJsonSerializer(Options.Create(new JsonSerializerOptions()));
        
        // Setup the cache to initially return null
        distributedCacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null);

        // Set a very short timeout to force timeouts
        var options = new FlowOptions { 
            ThunderingHerdProtectionTimeout = TimeSpan.FromMilliseconds(50),
            SuppressCacheExceptions = true 
        };
        
        var cache = new DistributedFlow(
            distributedCacheMock.Object, 
            serializer, 
            new Mock<ILogger<DistributedFlow>>().Object,
            Options.Create(options)
        );

        // Create a counter to track factory calls
        var factoryCalls = 0;
        var completionSource = new TaskCompletionSource<bool>();
        
        // Factory that will hang on the first call but complete quickly on subsequent calls
        async Task<string> FactoryMethod()
        {
            var callNumber = Interlocked.Increment(ref factoryCalls);
            
            if (callNumber == 1)
            {
                // First call will hang until the test completes it
                await completionSource.Task;
                return "first-result";
            }
            
            // Subsequent calls return quickly with a different result
            return "subsequent-result";
        }

        // Act
        // Start a task that will hang
        var firstTask = cache.GetOrSetAsync("test-key", FactoryMethod, TimeSpan.FromMinutes(5));
        
        // Wait a bit to ensure first task is running
        await Task.Delay(10);
        
        // Start a second task that should timeout waiting for the first one
        var secondTask = cache.GetOrSetAsync("test-key", FactoryMethod, TimeSpan.FromMinutes(5));
        
        // Complete the first factory method after a delay
        await Task.Delay(100);
        completionSource.SetResult(true);
        
        // Wait for both tasks
        var firstResult = await firstTask;
        var secondResult = await secondTask;

        // Assert
        Assert.Equal(2, factoryCalls); // Factory called twice due to timeout
        Assert.Equal("first-result", firstResult);
        Assert.Equal("subsequent-result", secondResult);
    }


    [Fact]
    public async Task GetOrSetAsync_WithPrefixAndConcurrentRequests_ShouldOnlyCallFactoryOnce()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serializer = new TextJsonSerializer(Options.Create(new JsonSerializerOptions()));

        distributedCacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null);

        distributedCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Use a non-empty prefix — this is what triggers the key mismatch bug
        var options = new FlowOptions
        {
            CacheKeyPrefix = "myapp",
            ThunderingHerdProtectionTimeout = TimeSpan.FromSeconds(5)
        };
        var cache = new DistributedFlow(
            distributedCacheMock.Object,
            serializer,
            new Mock<ILogger<DistributedFlow>>().Object,
            Options.Create(options)
        );

        var factoryCalls = 0;

        async Task<string> FactoryMethod()
        {
            Interlocked.Increment(ref factoryCalls);
            await Task.Delay(200);
            return "cached-value";
        }

        // Act
        const int concurrentRequests = 10;
        var tasks = new Task<string>[concurrentRequests];
        for (var i = 0; i < concurrentRequests; i++)
            tasks[i] = cache.GetOrSetAsync("test-key", FactoryMethod, TimeSpan.FromMinutes(5));

        var results = await Task.WhenAll(tasks);

        // Assert — factory must be called exactly once regardless of prefix
        Assert.Equal(1, factoryCalls);
        foreach (var result in results)
            Assert.Equal("cached-value", result);
    }
}
