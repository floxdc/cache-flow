using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow;

public class MemoryFlow<TClass> : FlowBase, IMemoryFlow<TClass> where TClass: class
{
    public MemoryFlow(IMemoryFlow cache, IOptions<FlowOptions> options)
    {
        _cache = cache;

        _keyPrefix = GetFullCacheKeyPrefix(typeof(TClass).FullName!, options.Value.CacheKeyDelimiter);
    }


    public IMemoryCache Instance 
        => _cache.Instance;

    public FlowOptions Options
        => _cache.Options;


    public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
        => GetOrSet(key, getValueFunction,
            new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


    public T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options)
        => _cache.GetOrSet(GetFullKey(_keyPrefix, key), getValueFunction, options);


    public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
        => GetOrSetAsync(GetFullKey(_keyPrefix, key), getValueFunction,
            new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
            cancellationToken);


    public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        => _cache.GetOrSetAsync(GetFullKey(_keyPrefix, key), getValueFunction, options, cancellationToken);


    public void Remove(string key)
        => _cache.Remove(GetFullKey(_keyPrefix, key));


    public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        => Set(GetFullKey(_keyPrefix, key), value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


    public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        => _cache.Set(GetFullKey(_keyPrefix, key), value, options);


    public bool TryGetValue<T>(string key, out T value)
        => _cache.TryGetValue(GetFullKey(_keyPrefix, key), out value);


    private readonly IMemoryFlow _cache;
    private readonly string _keyPrefix;
}