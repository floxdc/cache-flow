using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace FloxDc.CacheFlow
{
    public interface IDoubleFlow
    {
        IDistributedCache DistributedInstance { get; }
        IMemoryCache MemoryInstance { get; }

        ValueTask<T> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);
        T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null);
        T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteDistributedExpirationRelativeToNow);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null, CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);
        void Refresh(string key);
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null);
        void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);
        bool TryGetValue<T>(string key, out T value, MemoryCacheEntryOptions memoryOptions);
        bool TryGetValue<T>(string key, out T value, TimeSpan absoluteDistributedExpirationRelativeToNow);
    }
}