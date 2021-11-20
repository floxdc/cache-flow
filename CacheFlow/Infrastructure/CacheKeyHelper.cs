namespace FloxDc.CacheFlow.Infrastructure
{
    internal static class CacheKeyHelper
    {
        internal static string GetFullCacheKeyPrefix(string prefix, string delimiter)
            => prefix == string.Empty ? string.Empty : string.Concat(prefix, delimiter);


        internal static string GetFullKey(string prefix, string key)
            => prefix == string.Empty ? key : string.Concat(prefix, key);
    }
}
