using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

internal class CacheEntry : ICacheEntry
{
    public CacheEntry(object key, Action<object> removeCallback)
    {
        Key = key;
        _removeCallback = removeCallback;
    }

    public object Key { get; }
    public object? Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public IList<IChangeToken> ExpirationTokens { get; } = [];
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
    public long? Size { get; set; }


    public bool Expired 
        => AbsoluteExpiration.HasValue && AbsoluteExpiration.Value <= DateTimeOffset.Now;


    IList<IChangeToken> ICacheEntry.ExpirationTokens 
        => throw new NotImplementedException();


    public void Dispose()
    {
        if (!_disposed)
        {
            _removeCallback(Key);
            _disposed = true;
        }
    }


    private readonly Action<object> _removeCallback;
    private bool _disposed;
}
