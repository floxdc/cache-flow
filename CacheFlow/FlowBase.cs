namespace FloxDc.CacheFlow;

public class FlowBase
{
    protected static string GetFullKey(string keyPrefix, string key) 
        => string.Concat(keyPrefix, key);


    protected static string GetFullCacheKeyPrefix(string typeName, string delimiter) 
        => string.Concat(typeName, delimiter);
}