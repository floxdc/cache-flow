using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class MemoryFlow<TClass> : IMemoryFlow<TClass> where TClass: class
    {
        public MemoryFlow(IMemoryFlow cache, IOptions<FlowOptions> options)
        {
            _cache = cache;

            _keyPrefix = GetFullCacheKeyPrefix(typeof(TClass).FullName, options.Value.CacheKeyDelimiter);
        }


        public IMemoryCache Instance 
            => _cache.Instance;


        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getValueFunction,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options)
            => _cache.GetOrSet(GetFullKey(key), getValueFunction, options);


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction,
            TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => GetOrSetAsync(GetFullKey(key), getValueFunction,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow},
                cancellationToken);


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options,
            CancellationToken cancellationToken = default)
            => _cache.GetOrSetAsync(GetFullKey(key), getValueFunction, options, cancellationToken);


        public void Remove(string key)
            => _cache.Remove(GetFullKey(key));


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
            => Set(GetFullKey(key), value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
            => _cache.Set(GetFullKey(key), value, options);


        public bool TryGetValue<T>(string key, out T value)
            => _cache.TryGetValue(GetFullKey(key), out value);


        private string GetFullKey(string key) 
            => string.Concat(_keyPrefix, key);


        private static string GetFullCacheKeyPrefix(string typeName, string delimiter) 
            => string.Concat(typeName, delimiter);


        private readonly IMemoryFlow _cache;
        private readonly string _keyPrefix;
    }
}
