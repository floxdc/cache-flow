using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CacheFlowTests;

public class DoubleFlowTests
{
    [Fact]
    public async Task GetAsync_ShouldNotGetValueWhenValueIsNull()
    {
        var storedValue = new DefaultClass(0);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DefaultClass) null)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = await cache.GetAsync<DefaultClass>("key", TimeSpan.Zero);

        Assert.Null(result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()), Times.Never);
        distributedFlowMock.Verify(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetAsync_ShouldGetValueWhenValueIsInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(true)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = await cache.GetAsync<DefaultClass>("key", TimeSpan.Zero);

        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()), Times.Never);
        distributedFlowMock.Verify(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task GetAsync_ShouldGetValueWhenValueIsInDistributedCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedValue)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = await cache.GetAsync<DefaultClass>("key", TimeSpan.Zero);

        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()), Times.Never);
        distributedFlowMock.Verify(f => f.GetAsync<DefaultClass>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public void GetOrSet_ShouldReturnValueWhenValueIsInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(true)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.Refresh(It.IsAny<string>()))
            .Verifiable();
        distributedFlowMock.Setup(f => f.GetOrSet(It.IsAny<string>(), It.IsAny<Func<DefaultClass>>(), It.IsAny<DistributedCacheEntryOptions>()))
            .Returns(storedValue)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = cache.GetOrSet("key", () => storedValue, TimeSpan.MaxValue);
            
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()), Times.Never);
        distributedFlowMock.Verify(f => f.Refresh(It.IsAny<string>()), Times.Once);
        distributedFlowMock.Verify(f => f.GetOrSet(It.IsAny<string>(), It.IsAny<Func<DefaultClass>>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Never);
    }


    [Fact]
    public void GetOrSet_ShouldReturnValueWhenValueIsNotInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.Refresh(It.IsAny<string>()))
            .Verifiable();
        distributedFlowMock.Setup(f => f.GetOrSet(It.IsAny<string>(), It.IsAny<Func<DefaultClass>>(), It.IsAny<DistributedCacheEntryOptions>()))
            .Returns(storedValue)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = cache.GetOrSet("key", () => storedValue, TimeSpan.MaxValue);
            
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        distributedFlowMock.Verify(f => f.Refresh(It.IsAny<string>()), Times.Never);
        distributedFlowMock.Verify(f => f.GetOrSet(It.IsAny<string>(), It.IsAny<Func<DefaultClass>>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
    }


    [Fact]
    public async Task GetOrSetAsync_ShouldReturnValueWhenValueIsInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(true)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        distributedFlowMock.Setup(f => f.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DefaultClass>>>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedValue)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = await cache.GetOrSetAsync("key", () => Task.FromResult(storedValue), TimeSpan.MaxValue, CancellationToken.None);
            
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<TimeSpan>()), Times.Never);
        distributedFlowMock.Verify(f => f.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        distributedFlowMock.Verify(f => f.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DefaultClass>>>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task GetOrSetAsync_ShouldReturnValueWhenValueIsNotInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Verifiable();

        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        distributedFlowMock.Setup(f => f.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DefaultClass>>>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedValue)
            .Verifiable();

        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var result = await cache.GetOrSetAsync("key", () => Task.FromResult(storedValue), TimeSpan.MaxValue);
            
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        distributedFlowMock.Verify(f => f.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        distributedFlowMock.Verify(f => f.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<DefaultClass>>>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public void TryGetValue_ShouldReturnFalseWhenValueIsNotInCache()
    {
        var storedValue = (DefaultClass) null;
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Verifiable();
            
        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
            
        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var isFound = cache.TryGetValue("key", out DefaultClass result, TimeSpan.MaxValue);

        Assert.False(isFound);
        Assert.Null(result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
        distributedFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
    }


    [Fact]
    public void TryGetValue_ShouldReturnTrueWhenValueInMemoryCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(true)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Verifiable();
            
        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
            
        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var isFound = cache.TryGetValue("key", out DefaultClass result, TimeSpan.MaxValue);

        Assert.True(isFound);
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
        distributedFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Never);
    }


    [Fact]
    public void TryGetValue_ShouldReturnTrueWhenValueInDistributedCache()
    {
        var storedValue = new DefaultClass(42);
            
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(false)
            .Verifiable();
        memoryFlowMock.Setup(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Verifiable();
            
        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.TryGetValue(It.IsAny<string>(), out storedValue))
            .Returns(true)
            .Verifiable();
            
        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        var isFound = cache.TryGetValue("key", out DefaultClass result, TimeSpan.MaxValue);

        Assert.True(isFound);
        Assert.Equal(storedValue, result);
        memoryFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
        memoryFlowMock.Verify(f => f.Set(It.IsAny<string>(), It.IsAny<DefaultClass>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
        distributedFlowMock.Verify(f => f.TryGetValue(It.IsAny<string>(), out storedValue), Times.Once);
    }


    [Fact]
    public async Task RemoveAsync_ShouldRemoveValuesFromBothCaches()
    {
        var memoryFlowMock = new Mock<IMemoryFlow>();
        memoryFlowMock.Setup(f => f.Remove(It.IsAny<string>()))
            .Verifiable();
            
        var distributedFlowMock = new Mock<IDistributedFlow>();
        distributedFlowMock.Setup(f => f.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Verifiable();
            
        var cache = new DoubleFlow(distributedFlowMock.Object, memoryFlowMock.Object);
        await cache.RemoveAsync("key");

        memoryFlowMock.Verify(f => f.Remove(It.IsAny<string>()), Times.Once);
        distributedFlowMock.Verify(f => f.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}