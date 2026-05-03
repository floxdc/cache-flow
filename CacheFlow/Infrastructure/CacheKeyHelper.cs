using System;

namespace FloxDc.CacheFlow.Infrastructure;

internal static class CacheKeyHelper
{
    internal static string GetFullCacheKeyPrefix(string prefix, string delimiter)
        => string.IsNullOrWhiteSpace(prefix) ? string.Empty : string.Concat(prefix, delimiter);


    internal static string GetFullKey(string prefix, string key) 
        => key is null
            ? throw new ArgumentNullException(nameof(key))
            : string.IsNullOrWhiteSpace(prefix) ? key : string.Concat(prefix, key);
}