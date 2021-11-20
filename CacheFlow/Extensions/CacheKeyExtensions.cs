namespace FloxDc.CacheFlow.Extensions;

public static class CacheKeyExtensions
{
    public static string BuildKey(this IDistributedFlow cache, params string[] keyComponents)
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    public static string BuildKey<T>(this IDistributedFlow<T> cache, params string[] keyComponents) where T: class
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    public static string BuildKey(this IDoubleFlow cache, params string[] keyComponents)
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    public static string BuildKey<T>(this IDoubleFlow<T> cache, params string[] keyComponents) where T: class
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    public static string BuildKey(this IMemoryFlow cache, params string[] keyComponents)
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    public static string BuildKey<T>(this IMemoryFlow<T> cache, params string[] keyComponents) where T: class
    {
        var delimiter = cache.Options.CacheKeyDelimiter;
        return BuildInternal(delimiter, keyComponents);
    }


    private static string BuildInternal(string delimiter, params string[] keyComponents) 
        => string.Join(delimiter, keyComponents);
}