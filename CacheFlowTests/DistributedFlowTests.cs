using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace CacheFlowTests
{
    public class DistributedFlowTests
    {
        [Fact]
        public void Refresh_ShouldRefreshCache()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Refresh(It.IsAny<string>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            cache.Refresh("Key");

            distributedCacheMock.Verify(c => c.Refresh(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task RefreshAsync_ShouldRefreshCache()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.RefreshAsync("Key");

            distributedCacheMock.Verify(c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public void Remove_ShouldRemove()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Remove(It.IsAny<string>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            cache.Remove("Key");

            distributedCacheMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task RemoveAsync_ShouldRemove()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.RemoveAsync("Key");

            distributedCacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public void Set_ShouldNotSetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            cache.Set("Key", new DefaultStruct(0), TimeSpan.MaxValue);

            distributedCacheMock.Verify(
                c => c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()),
                Times.Never);
        }


        [Fact]
        public void Set_ShouldSetValueWhenValueIsDefaultPrimitiveStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            cache.Set("Key", default(int), TimeSpan.MaxValue);

            distributedCacheMock
                .Verify(c => c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
        }


        [Fact]
        public async Task SetAsync_ShouldNotSetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.SetAsync("Key", new DefaultStruct(0), TimeSpan.MaxValue);

            distributedCacheMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task SetAsync_ShouldSetValueWhenValueIsDefaultPrimitiveStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.SetAsync("Key", default(int), TimeSpan.MaxValue);

            distributedCacheMock
                .Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
