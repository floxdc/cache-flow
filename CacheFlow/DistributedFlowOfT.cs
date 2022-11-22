using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DistributedFlow<TClass> : FlowBase, IDistributedFlow<TClass> where TClass: class
    {
        public DistributedFlow(IDistributedFlow cache, IOptions<FlowOptions> options)
        {
            _cache = cache;

            _keyPrefix = GetFullCacheKeyPrefix(typeof(TClass).FullName!, options.Value.CacheKeyDelimiter);
        }


        public IDistributedCache Instance
            => _cache.Instance;

        public FlowOptions Options
            => _cache.Options;


        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
            => _cache.GetAsync<T>(GetFullKey(_keyPrefix, key), cancellationToken);


        public T? GetOrSet<T>(string key, Func<T> getValueFunction, in TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getValueFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T? GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions options)
            => _cache.GetOrSet(GetFullKey(_keyPrefix, key), getValueFunction, options);


        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, getValueFunction, new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
            => _cache.GetOrSetAsync(GetFullKey(_keyPrefix, key), getValueFunction, options, cancellationToken);


        public void Refresh(string key)
            => _cache.Refresh(GetFullKey(_keyPrefix, key));


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
            => _cache.RefreshAsync(GetFullKey(_keyPrefix, key), cancellationToken);


        public void Remove(string key)
            => _cache.Remove(GetFullKey(_keyPrefix, key));


        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            => _cache.RemoveAsync(GetFullKey(_keyPrefix, key), cancellationToken);


        public void Set<T>(string key, T value, in TimeSpan absoluteExpirationRelativeToNow)
            => Set(key, value, new DistributedCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
            => _cache.Set(GetFullKey(_keyPrefix, key), value, options);


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
            => SetAsync(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow}, cancellationToken);


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
            => _cache.SetAsync(GetFullKey(_keyPrefix, key), value, options, cancellationToken);


        public bool TryGetValue<T>(string key, out T? value)
            => _cache.TryGetValue(GetFullKey(_keyPrefix, key), out value);


        private readonly IDistributedFlow _cache;
        private readonly string _keyPrefix;
    }
}
