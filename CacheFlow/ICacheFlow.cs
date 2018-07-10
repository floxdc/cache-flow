using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FloxDc.CacheFlow
{
    public interface ICacheFlow
    {
        Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);
        T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow);
        T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions options);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);
        void RefreshCache(string key);
        Task RefreshCacheAsync(string key, CancellationToken cancellationToken = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);
        void Set<T>(string key, T value, DistributedCacheEntryOptions options);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);
        bool TryGetValue<T>(string key, out T value);
    }
}
