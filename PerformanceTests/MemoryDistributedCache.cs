using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace PerformanceTests;

public class MemoryDistributedCache : IDistributedCache
{
    public MemoryDistributedCache()
    {
        _memoryCache = new MemoryCache();
    }


    public byte[] Get(string key)
    {
        return _memoryCache.TryGetValue(key, out byte[] value) ? value : null;
    }


    public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        return await Task.FromResult(Get(key));
    }


    public void Refresh(string key)
    {
        // No-op for in-memory cache
    }


    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        await Task.CompletedTask;
    }


    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }


    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        await Task.CompletedTask;
    }


    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var memoryCacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration
        };
        _memoryCache.Set(key, value, memoryCacheEntryOptions);
    }


    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        await Task.CompletedTask;
    }


    private readonly IMemoryCache _memoryCache;
}
