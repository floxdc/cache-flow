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

        /// <summary>
        /// Gets value from cache.
        /// </summary>
        /// <typeparam name="T">Type of extracted value.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<T> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="distributedOptions"></param>
        /// <param name="memoryOptions"></param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null);

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow);

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="distributedOptions"></param>
        /// <param name="memoryOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to get value from cache, and sets it if no entry was found.
        /// </summary>
        /// <typeparam name="T">Type of getted and setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="getValueFunction">Function what gets value to set to cache. Executes if provided key wasn't found.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes specific cache entry.
        /// </summary>
        /// <param name="key">Target cache key.</param>
        void Refresh(string key);

        /// <summary>
        /// Refreshes specific cache entry.
        /// </summary>
        /// <param name="key">Target cache key.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes specific cache entry.
        /// </summary>
        /// <param name="key">Target cache key.</param>
        void Remove(string key);

        /// <summary>
        /// Removes specific cache entry.
        /// </summary>
        /// <param name="key">Target cache key.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="distributedOptions"></param>
        /// <param name="memoryOptions"></param>
        void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null);

        /// <summary>
        /// Sets cache entry with provided value.
        /// </summary>
        /// <typeparam name="T">Type of setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="value">Value of the cache entry.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow);

        /// <summary>
        /// Sets cache entry with provided value.
        /// </summary>
        /// <typeparam name="T">Type of setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="value">Value of the cache entry.</param>
        /// <param name="distributedOptions"></param>
        /// <param name="memoryOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets cache entry with provided value.
        /// </summary>
        /// <typeparam name="T">Type of setted value.</typeparam>
        /// <param name="key">Target cache key.</param>
        /// <param name="value">Value of the cache entry.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to get value from cache.
        /// </summary>
        /// <typeparam name="T">Type of extracted value.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Extracted value.</param>
        /// <param name="memoryOptions"></param>
        /// <returns></returns>
        bool TryGetValue<T>(string key, out T value, MemoryCacheEntryOptions memoryOptions);

        /// <summary>
        /// Tries to get value from cache.
        /// </summary>
        /// <typeparam name="T">Type of extracted value.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Extracted value.</param>
        /// <param name="absoluteDistributedExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire cache.</param>
        /// <returns></returns>
        bool TryGetValue<T>(string key, out T value, TimeSpan absoluteDistributedExpirationRelativeToNow);
    }
}