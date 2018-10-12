using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace FloxDc.CacheFlow
{
    public interface IMemoryFlow
    {
        IMemoryCache Instance { get; }

        T GetOrSet<T>(string key, Func<T> getFunction, MemoryCacheEntryOptions options);
        T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteExpirationRelativeToNow);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, MemoryCacheEntryOptions options);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan absoluteExpirationRelativeToNow);
        void Remove(string key);
        void Set<T>(string key, T value, MemoryCacheEntryOptions options);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        bool TryGetValue<T>(string key, out T value);
    }
}