using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FloxDc.CacheFlow
{
    public interface ICacheFlow
    {
        Task<T> GetValueAsync<T>(string key);
        T GetOrSet<T>(string key, TimeSpan absoluteExpirationRelativeToNow, Func<T> getFunction);
        T GetOrSet<T>(string key, DistributedCacheEntryOptions options, Func<T> getFunction);
        Task<T> GetOrSetAsync<T>(string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<T>> getFunction);
        Task<T> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T>> getFunction);
        void Remove(string key);
        Task RemoveAsync(string key);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        void Set<T>(string key, T value, DistributedCacheEntryOptions options);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options);
        bool TryGetValue<T>(string key, out T value);
    }
}
