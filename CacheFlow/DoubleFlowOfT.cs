using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DoubleFlow<TClass> : FlowBase, IDoubleFlow<TClass> where TClass : class
    {
        public DoubleFlow(IDoubleFlow cache, IOptions<FlowOptions> options)
        {
            _cache = cache;

            _keyPrefix = GetFullCacheKeyPrefix(typeof(TClass).FullName!, options.Value.CacheKeyDelimiter);
        }


        public IDistributedCache DistributedInstance => _cache.DistributedInstance;
        public IMemoryCache MemoryInstance => _cache.MemoryInstance;
        public FlowOptions Options => _cache.Options;


        public ValueTask<T?> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => _cache.GetAsync<T>(GetFullKey(_keyPrefix, key), absoluteDistributedExpirationRelativeToNow, cancellationToken);


        public T GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions distributedOptions,
            MemoryCacheEntryOptions? memoryOptions = null)
            => _cache.GetOrSet(GetFullKey(_keyPrefix, key), getValueFunction, distributedOptions, memoryOptions);


        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => GetOrSet(GetFullKey(_keyPrefix, key), getValueFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow});


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions distributedOptions,
            MemoryCacheEntryOptions? memoryOptions = null,
            CancellationToken cancellationToken = default)
            => _cache.GetOrSetAsync(GetFullKey(_keyPrefix, key), getValueFunction, distributedOptions, memoryOptions, cancellationToken);


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => GetOrSetAsync(GetFullKey(_keyPrefix, key), getValueFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow}, cancellationToken);


        public void Refresh(string key) => _cache.Refresh(GetFullKey(_keyPrefix, key));


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
            => _cache.RefreshAsync(GetFullKey(_keyPrefix, key), cancellationToken);


        public void Remove(string key) => _cache.Remove(GetFullKey(_keyPrefix, key));


        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            => _cache.RemoveAsync(GetFullKey(_keyPrefix, key), cancellationToken);


        public void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null)
        {
            _cache.Set(GetFullKey(_keyPrefix, key), value, distributedOptions, memoryOptions);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
        {
            Set(GetFullKey(_keyPrefix, key), value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow});
        }


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions? memoryOptions = null,
            CancellationToken cancellationToken = default)
            => _cache.SetAsync(GetFullKey(_keyPrefix, key), value, distributedOptions, memoryOptions, cancellationToken);


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => SetAsync(GetFullKey(_keyPrefix, key), value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow}, cancellationToken);


        public bool TryGetValue<T>(string key, out T? value, MemoryCacheEntryOptions memoryOptions)
            => _cache.TryGetValue(GetFullKey(_keyPrefix, key), out value, memoryOptions);


        public bool TryGetValue<T>(string key, out T? value, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => TryGetValue(GetFullKey(_keyPrefix, key), out value,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow});


        private readonly IDoubleFlow _cache;
        private readonly string _keyPrefix;
    }
}