using System;
using FloxDc.CacheFlow;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CacheFlowTests
{
    public class MemoryFlowTests
    {
        [Fact]
        public void Remove_ShouldRemoveValue()
        {
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.Remove(It.IsAny<object>()))
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            cache.Remove("key");

            memoryCacheMock.Verify(c => c.Remove(It.IsAny<object>()), Times.Once);
        }


        [Fact]
        public void Set_ShouldNotSetValueWhenValueIsDefaultUserDefinedStruct()
        {
            var entryMock = new Mock<ICacheEntry>();
            entryMock.SetupSet(e => e.Value = It.IsAny<object>())
                .Verifiable();

            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(entryMock.Object)
                .Verifiable();

            var value = default(DefaultStruct);
            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            cache.Set("key", value, TimeSpan.MaxValue);

            entryMock.VerifySet(e => e.Value = It.IsAny<object>(), Times.Never);
            memoryCacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Never);
        }


        [Fact]
        public void Set_ShouldSetValueWhenValueIsDefaultPrimitiveStruct()
        {
            var entryMock = new Mock<ICacheEntry>();
            entryMock.SetupSet(e => e.Value = It.IsAny<object>())
                .Verifiable();

            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(entryMock.Object)
                .Verifiable();

            var value = default(int);
            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            cache.Set("key", value, TimeSpan.MaxValue);

            entryMock.VerifySet(e => e.Value = It.IsAny<object>(), Times.Once);
            memoryCacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
        }


        [Fact]
        public void Set_ShouldSetValueWhenValueIsNull()
        {
            var entryMock = new Mock<ICacheEntry>();
            entryMock.SetupSet(e => e.Value = It.IsAny<object>())
                .Verifiable();

            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(entryMock.Object)
                .Verifiable();

            var value = (DefaultClass) null;
            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            cache.Set("key", value, TimeSpan.MaxValue);

            entryMock.VerifySet(e => e.Value = It.IsAny<object>(), Times.Once);
            memoryCacheMock.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldNotGetValueWhenValueIsNull()
        {
            object storedValue;
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out storedValue))
                .Returns(false)
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            var expected = cache.TryGetValue<object>("key", out var value);

            Assert.False(expected);
            Assert.Null(value);
            memoryCacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out storedValue), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldNotGetValueWhenValueIsDefaultUserDefinedStruct()
        {
            object storedValue;
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out storedValue))
                .Returns(false)
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            var expected = cache.TryGetValue("key", out DefaultStruct value);

            Assert.False(expected);
            Assert.Equal(default, value);
            memoryCacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out storedValue), Times.Once);
        }


        [Theory]
        [InlineData(default(int))]
        [InlineData(42)]
        public void TryGetValue_ShouldGetValueWhenValueIsPrimitiveStruct(int storedValue)
        {
            var temp = (object) storedValue;
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out temp))
                .Returns(true)
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            var expected = cache.TryGetValue("key", out int value);

            Assert.True(expected);
            Assert.Equal(storedValue, value);
            memoryCacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out temp), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldGetValueWhenValueIsUserDefinedStruct()
        {
            var storedValue = new DefaultStruct(42);
            var temp = (object) storedValue;
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out temp))
                .Returns(true)
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            var expected = cache.TryGetValue("key", out DefaultStruct value);

            Assert.True(expected);
            Assert.Equal(storedValue, value);
            memoryCacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out temp), Times.Once);
        }


        [Fact]
        public void TryGetValue_ShouldGetValueWhenValueIsClass()
        {
            var storedValue = new DefaultClass(42);
            var temp = (object)storedValue;
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out temp))
                .Returns(true)
                .Verifiable();

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object);
            var expected = cache.TryGetValue("key", out DefaultClass value);

            Assert.True(expected);
            Assert.Equal(storedValue, value);
            memoryCacheMock.Verify(c => c.TryGetValue(It.IsAny<object>(), out temp), Times.Once);
        }


        [Fact]
        public void ShouldFindKeyWithPredefinedPrefix()
        {
            const string prefix = "Prefix";
            const string delimiter = "-";
            const string key = "Key";
            var obj = new object();
            var memoryCacheMock = new Mock<IMemoryCache>();
            memoryCacheMock.Setup(c => c.TryGetValue(prefix + delimiter + key, out obj))
                .Returns(true);
            var optionsMock = new Mock<IOptions<FlowOptions>>();
            optionsMock.Setup(o => o.Value)
                .Returns(new FlowOptions{CacheKeyDelimiter = delimiter, CacheKeyPrefix = prefix});

            var cache = new MemoryFlow(new TestDiagnosticSource(), memoryCacheMock.Object, options: optionsMock.Object);
            var expected = cache.TryGetValue(key, out object value);

            Assert.True(expected);
            Assert.Equal(obj, value);
        }
    }
}
