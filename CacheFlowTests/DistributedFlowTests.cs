using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
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
                .Verify(c => c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()),
                    Times.Once);
        }


        [Fact]
        public async Task SetAsync_ShouldNotSetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.SetAsync("Key", new DefaultStruct(0), TimeSpan.MaxValue);

            distributedCacheMock
                .Verify(
                    c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
        }


        [Fact]
        public async Task SetAsync_ShouldSetValueWhenValueIsDefaultPrimitiveStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            await cache.SetAsync("Key", default(int), TimeSpan.MaxValue);

            distributedCacheMock
                .Verify(
                    c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldNotGetValueWhenValueIsNull()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
                .Returns((byte[]) null)
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = cache.TryGetValue<object>("key", out _);

            Assert.False(result);
            distributedCacheMock.Verify(c => c.Get(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldNotGetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
                .Returns((byte[])null)
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var isSuccess = cache.TryGetValue("key", out DefaultStruct result);

            Assert.False(isSuccess);
            Assert.Equal(default, result);
            distributedCacheMock.Verify(c => c.Get(It.IsAny<string>()), Times.Once);
        }


        [Theory]
        [InlineData(default(int))]
        [InlineData(42)]
        public void TryGetValue_ShouldGetValueWhenValueIsPrimitiveStruct(int storedValue)
        {
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var isSuccess = cache.TryGetValue("key", out int result);

            Assert.True(isSuccess);
            Assert.Equal(storedValue, result);
            distributedCacheMock.Verify(c => c.Get(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldGetValueWhenValueIsUserDefinedStruct()
        {
            var storedValue = new DefaultStruct(42);
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var isSuccess = cache.TryGetValue("key", out DefaultStruct result);

            Assert.True(isSuccess);
            Assert.Equal(storedValue, result);
            distributedCacheMock.Verify(c => c.Get(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldGetValueWhenValueIsClass()
        {
            var storedValue = new DefaultClass(42);
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var isSuccess = cache.TryGetValue("key", out DefaultClass result);

            Assert.True(isSuccess);
            Assert.Equal(storedValue.Id, result.Id);
            distributedCacheMock.Verify(c => c.Get(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task GetAsync_ShouldNotGetValueWhenValueIsNull()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null)
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetAsync<object>("key");

            Assert.Null(result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetAsync_ShouldNotGetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null)
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetAsync<DefaultStruct>("key");

            Assert.Equal(default, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Theory]
        [InlineData(default(int))]
        [InlineData(42)]
        public async Task GetAsync_ShouldGetValueWhenValueIsPrimitiveStruct(int storedValue)
        {
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetAsync<int>("key");

            Assert.Equal(storedValue, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetAsync_ShouldGetValueWhenValueIsUserDefinedStruct()
        {
            var storedValue = new DefaultStruct(42);
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetAsync<DefaultStruct>("key");

            Assert.Equal(storedValue, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetAsync_ShouldGetValueWhenValueIsClass()
        {
            var storedValue = new DefaultClass(42);
            var temp = (object)storedValue;
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ObjectToByteArray(temp))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetAsync<DefaultClass>("key");

            Assert.Equal(storedValue, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetOrSetAsync_ShouldExecutesFunctionWhenGetAsyncReturnsDefaultStruct()
        {
            var storedValue = default(DefaultStruct);
            var temp = (object) storedValue;
            var returnedValue = new DefaultStruct(42);
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ObjectToByteArray(temp))
                .Verifiable();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetOrSetAsync("key", async () => await Task.FromResult(returnedValue),
                TimeSpan.FromMilliseconds(1));

            Assert.Equal(returnedValue, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            distributedCacheMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetOrSetAsync_ShouldExecutesFunctionWhenGetAsyncReturnsDefaultClass()
        {
            var returnedValue = new DefaultClass(42);
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ObjectToByteArray(null))
                .Verifiable();
            distributedCacheMock.Setup(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()))
                .Verifiable();

            var cache = new DistributedFlow(distributedCacheMock.Object);
            var result = await cache.GetOrSetAsync("key", async () => await Task.FromResult(returnedValue),
                TimeSpan.FromMilliseconds(1));

            Assert.Equal(returnedValue, result);
            distributedCacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            distributedCacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public void ShouldFindKeyWithPredefinedPrefix()
        {
            const string prefix = "Prefix";
            const string delimiter = "-";
            const string key = "Key";
            var obj = new DefaultClass(42);
            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock.Setup(c => c.Get(prefix + delimiter + key))
                .Returns(ObjectToByteArray(obj));
            var optionsMock = new Mock<IOptions<FlowOptions>>();
            optionsMock.Setup(o => o.Value)
                .Returns(new FlowOptions { CacheKeyDelimiter = delimiter, CacheKeyPrefix = prefix });

            var cache = new DistributedFlow(distributedCacheMock.Object, options: optionsMock.Object);
            var expected = cache.TryGetValue(key, out object value);

            Assert.True(expected);
            Assert.Equal(obj, value);
        }


        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
