using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace FloxDc.CacheFlow
{
    public interface IMemoryFlow : IFlow
    {
        IMemoryCache Instance { get; }

        T GetOrSet<T>(string key, Func<T> getFunction, MemoryCacheEntryOptions options);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, MemoryCacheEntryOptions options);
        void Set<T>(string key, T value, MemoryCacheEntryOptions options);
    }
}