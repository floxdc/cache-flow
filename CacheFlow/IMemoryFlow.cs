using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace FloxDc.CacheFlow;

public interface IMemoryFlow : IMemoryFlow<string> { }

public interface IMemoryFlow<TClass> where TClass : class
{
    IMemoryCache Instance { get; }
    FlowOptions Options { get; }

    /// <summary>
    /// Tries to get a value from the cache and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of the value to get and set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function that gets a value to set in the cache. Executes if the provided key wasn't found.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire the cache.</param>
    /// <returns></returns>
    T GetOrSet<T>(string key, Func<T> getValueFunction, in TimeSpan absoluteExpirationRelativeToNow);

    /// <summary>
    /// Tries to get a value from the cache and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of the value to get and set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function that gets a value to set in the cache. Executes if the provided key wasn't found.</param>
    /// <param name="options">Cache options.</param>
    /// <returns></returns>
    T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options);

    /// <summary>
    /// Tries to get a value from the cache and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of the value to get and set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function that gets a value to set in the cache. Executes if the provided key wasn't found.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire the cache.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to get a value from the cache and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of the value to get and set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function that gets a value to set in the cache. Executes if the provided key wasn't found.</param>
    /// <param name="options">Cache options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specified cache entry.
    /// </summary>
    /// <param name="key">Target cache key.</param>
    void Remove(string key);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of the value to set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire the cache.</param>
    void Set<T>(string key, T value, in TimeSpan absoluteExpirationRelativeToNow);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of the value to set.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="options">Cache options.</param>
    void Set<T>(string key, T value, MemoryCacheEntryOptions options);

    /// <summary>
    /// Tries to get a value from the cache.
    /// </summary>
    /// <typeparam name="T">Type of the extracted value.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Extracted value.</param>
    /// <returns></returns>
    bool TryGetValue<T>(string key, out T value);
}