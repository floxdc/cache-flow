﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FloxDc.CacheFlow;

public interface IDistributedFlow : IDistributedFlow<string> { }

public interface IDistributedFlow<TClass> where  TClass: class
{
    IDistributedCache Instance { get; }
    FlowOptions Options { get; }

    /// <summary>
    /// Gets a value from a cache.
    /// </summary>
    /// <typeparam name="T">Type of an extracted value.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to get a value from a cache, and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of a got and a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function what gets value to set it to a cache. Executes if a provided key wasn't found.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire a cache.</param>
    /// <returns></returns>
    T? GetOrSet<T>(string key, Func<T> getValueFunction, in TimeSpan absoluteExpirationRelativeToNow);
        
    /// <summary>
    /// Tries to get a value from a cache, and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of a got and a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function what gets a value to set it to a cache. Executes if a provided key wasn't found.</param>
    /// <param name="options">Cache options.</param>
    /// <returns></returns>
    T? GetOrSet<T>(string key, Func<T> getValueFunction, DistributedCacheEntryOptions options);

    /// <summary>
    /// Tries to get a value from a cache, and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of a got and a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function what gets a value to set it to a cache. Executes if a provided key wasn't found.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire a cache.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to get a value from a cache, and sets it if no entries were found.
    /// </summary>
    /// <typeparam name="T">Type of a got and a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="getValueFunction">Function what gets a value to set it to a cache. Executes if a provided key wasn't found.</param>
    /// <param name="options">Cache options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes a specified cache entry.
    /// </summary>
    /// <param name="key">Target cache key.</param>
    void Refresh(string key);

    /// <summary>
    /// Refreshes a specified cache entry.
    /// </summary>
    /// <param name="key">Target cache key.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specified cache entry.
    /// </summary>
    /// <param name="key">Target cache key.</param>
    void Remove(string key);

    /// <summary>
    /// Removes a specified cache entry.
    /// </summary>
    /// <param name="key">Target cache key.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire a cache.</param>
    void Set<T>(string key, T value, in TimeSpan absoluteExpirationRelativeToNow);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="options">Cache options.</param>
    void Set<T>(string key, T value, DistributedCacheEntryOptions options);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="absoluteExpirationRelativeToNow">Absolute amount of time relative to now which should pass to expire a cache.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cache entry with a provided value.
    /// </summary>
    /// <typeparam name="T">Type of a set value.</typeparam>
    /// <param name="key">Target cache key.</param>
    /// <param name="value">Value of the cache entry.</param>
    /// <param name="options">Cache options.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Tries to get a value from a cache.
    /// </summary>
    /// <typeparam name="T">Type of an extracted value.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Extracted value.</param>
    /// <returns></returns>
    bool TryGetValue<T>(string key, out T? value);
}