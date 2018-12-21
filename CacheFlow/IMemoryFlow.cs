using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace FloxDc.CacheFlow
{
    public interface IMemoryFlow : IFlow
    {
        IMemoryCache Instance { get; }

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="options">Cache options.</param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options);

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="options">Cache options.</param>
        /// <returns></returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options);

        /// <summary>
        /// Sets cache entry with provided value.
        /// </summary>
        /// <typeparam name="T">Type of setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="value">Value of the cache entry.</param>
        /// <param name="options">Cache options.</param>
        void Set<T>(string key, T value, MemoryCacheEntryOptions options);
    }
}