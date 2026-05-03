using System;
using FloxDc.CacheFlow.Infrastructure;
using Xunit;

namespace CacheFlowTests;

public class CacheKeyHelperTests
{
    [Fact]
    public void GetFullCacheKeyPrefix_WhenPrefixIsEmpty_ShouldReturnEmpty()
        => Assert.Equal(string.Empty, CacheKeyHelper.GetFullCacheKeyPrefix(string.Empty, "::"));


    [Fact]
    public void GetFullCacheKeyPrefix_WhenPrefixIsNonEmpty_ShouldAppendDelimiter()
        => Assert.Equal("myapp::", CacheKeyHelper.GetFullCacheKeyPrefix("myapp", "::"));


    [Fact]
    public void GetFullCacheKeyPrefix_WhenPrefixIsNull_ShouldReturnEmpty()
        => Assert.Equal(string.Empty, CacheKeyHelper.GetFullCacheKeyPrefix(null!, "::"));


    [Fact]
    public void GetFullCacheKeyPrefix_WhenPrefixIsWhitespace_ShouldReturnEmpty()
        => Assert.Equal(string.Empty, CacheKeyHelper.GetFullCacheKeyPrefix("   ", "::"));


    [Fact]
    public void GetFullKey_WhenPrefixIsEmpty_ShouldReturnKeyOnly()
        => Assert.Equal("my-key", CacheKeyHelper.GetFullKey(string.Empty, "my-key"));


    [Fact]
    public void GetFullKey_WhenPrefixIsNonEmpty_ShouldPrependPrefix()
        => Assert.Equal("myapp::my-key", CacheKeyHelper.GetFullKey("myapp::", "my-key"));


    [Fact]
    public void GetFullKey_WhenPrefixIsNull_ShouldReturnKeyOnly()
        => Assert.Equal("my-key", CacheKeyHelper.GetFullKey(null!, "my-key"));


    [Fact]
    public void GetFullKey_WhenPrefixIsWhitespace_ShouldReturnKeyOnly()
        => Assert.Equal("my-key", CacheKeyHelper.GetFullKey("   ", "my-key"));


    [Fact]
    public void GetFullKey_WhenKeyIsNull_ShouldThrowArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => CacheKeyHelper.GetFullKey(string.Empty, null!));
}
