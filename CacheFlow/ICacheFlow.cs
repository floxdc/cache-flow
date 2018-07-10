using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FloxDc.CacheFlow
{
    public interface ICacheFlow
    {
        Task<T> GetValueAsync<T>(string key);
        T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow);
        T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan absoluteExpirationRelativeToNow);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, DistributedCacheEntryOptions options);
        void Remove(string key);
        Task RemoveAsync(string key);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        void Set<T>(string key, T value, DistributedCacheEntryOptions options);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options);
        bool TryGetValue<T>(string key, out T value);
    }
}
