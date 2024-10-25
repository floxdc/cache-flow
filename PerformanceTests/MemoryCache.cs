using Microsoft.Extensions.Caching.Memory;

public class MemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(MemoryCache));

        var entry = new CacheEntry(key, RemoveEntry);
        _cache[key] = entry;
        return entry;
    }


    public void Remove(object key)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(MemoryCache));

        _cache.Remove(key);
    }


    public bool TryGetValue(object key, out object? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(MemoryCache));

        if (_cache.TryGetValue(key, out var entry) && !IsExpired(entry))
        {
            value = entry.Value;
            return true;
        }

        value = null;
        return false;
    }


    private static bool IsExpired(ICacheEntry entry) 
        => entry.AbsoluteExpiration.HasValue && entry.AbsoluteExpiration.Value <= DateTimeOffset.Now;


    public void Dispose()
    {
        if (!_disposed)
        {
            _cache.Clear();
            _disposed = true;
        }
    }

    private void RemoveEntry(object key) 
        => _cache.Remove(key);


    private readonly Dictionary<object, ICacheEntry> _cache = [];
    private bool _disposed;
}
