using System;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging;

internal static partial class LoggerMessages
{
    [LoggerMessage((int)CacheEvent.Hit, LogLevel.Debug, "HIT | {target} | {Key} | {value} | CacheFlow: A cache hit occurred.")]
    public static partial void CacheHit(this ILogger logger, string target, string key, object value);

    [LoggerMessage((int)CacheEvent.Hit, LogLevel.Debug, "HIT | {Key} | CacheFlow: A cache hit occurred.")]
    public static partial void CacheHitInsensitive(this ILogger logger, string key);

    [LoggerMessage((int)CacheEvent.Miss, LogLevel.Debug, "MISS | {target} | {Key} | CacheFlow: A cache miss occurred.")]
    public static partial void CacheMissed(this ILogger logger, string target, string key);

    [LoggerMessage((int)CacheEvent.Miss, LogLevel.Debug, "MISS | {Key} | CacheFlow: A cache miss occurred.")]
    public static partial void CacheMissedInsensitive(this ILogger logger, string key);

    [LoggerMessage((int)CacheEvent.Skipped, LogLevel.Debug, "SKIPPED | {target} | {Key} | {value} | CacheFlow: The key has not been set because the entry is a default struct value.")]
    public static partial void CacheNotSet(this ILogger logger, string target, string key, object value);

    [LoggerMessage((int)CacheEvent.Skipped, LogLevel.Debug, "SKIPPED | {Key} | CacheFlow: The key has not been set because the entry is a default struct value.")]
    public static partial void CacheNotSetInsensitive(this ILogger logger, string key);

    [LoggerMessage((int)CacheEvent.Set, LogLevel.Debug, "SET | {target} | {Key} | {value} | CacheFlow: The entry has been set.")]
    public static partial void CacheSet(this ILogger logger, string target, string key, object value);

    [LoggerMessage((int)CacheEvent.Set, LogLevel.Debug, "SET | {Key} | CacheFlow: The entry has been set.")]
    public static partial void CacheSetInsensitive(this ILogger logger, string key);

    [LoggerMessage((int)CacheEvent.Remove, LogLevel.Debug, "REMOVED | {target} | {Key} | CacheFlow: The key has been removed from the cache.")]
    public static partial void EntryRemoved(this ILogger logger, string target, string key);

    [LoggerMessage((int)CacheEvent.Remove, LogLevel.Debug, "REMOVED | {Key} | CacheFlow: The key has been removed from the cache.")]
    public static partial void EntryRemovedInsensitive(this ILogger logger, string key);

    [LoggerMessage((int)CacheEvent.AnErrorHasOccurred, LogLevel.Error, "EXCEPTION | {target} | CacheFlow: An error occurred.")]
    public static partial void ErrorOccurred(this ILogger logger, string target, Exception exception);

    [LoggerMessage((int)CacheEvent.AnErrorHasOccurred, LogLevel.Error, "EXCEPTION | CacheFlow: An error occurred.")]
    public static partial void ErrorOccurredInsensitive(this ILogger logger, Exception exception);

    [LoggerMessage((int)CacheEvent.AnErrorHasOccurred, LogLevel.Warning, "NO OPTIONS | {target} | CacheFlow: No options have been provided. The defaults are used.")]
    public static partial void NoOptions(this ILogger logger, string target);
}
