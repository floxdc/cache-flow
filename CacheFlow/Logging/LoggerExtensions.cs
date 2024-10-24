using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging;

internal static class LoggerExtensions
{
    static LoggerExtensions()
    {
        CacheHit = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int)CacheEvent.Hit, CacheEvent.Hit.ToString()), "HIT | {target} | {Key} | {value} | CacheFlow: The cache hit occurs.");
        CacheHitInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvent.Hit, CacheEvent.Hit.ToString()), "HIT | {Key} | CacheFlow: The cache hit occurs.");
            
        CacheMissed = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId((int)CacheEvent.Miss, CacheEvent.Miss.ToString()), "MISS | {target} | {Key} | CacheFlow: The cache miss occurs.");
        CacheMissedInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvent.Miss, CacheEvent.Miss.ToString()), "MISS | {Key} | CacheFlow: The cache miss occurs.");
            
        CacheNotSet = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int) CacheEvent.Skipped, CacheEvent.Skipped.ToString()), "SKIPPED | {target} | {Key} | {value} | CacheFlow: The key has not been set, because the entry is a default struct value.");
        CacheNotSetInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int) CacheEvent.Skipped, CacheEvent.Skipped.ToString()), "SKIPPED | {Key} | CacheFlow: The key has not been set, because the entry is a default struct value.");
            
        CacheSet = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int)CacheEvent.Set, CacheEvent.Set.ToString()), "SET | {target} | {Key} | {value} | CacheFlow: The entry is set.");
        CacheSetInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvent.Set, CacheEvent.Set.ToString()), "SET | {Key} | CacheFlow: The entry is set.");
            
        ErrorOccurred = LoggerMessage.Define<string>(LogLevel.Error, new EventId((int)CacheEvent.AnErrorHasOccurred, CacheEvent.AnErrorHasOccurred.ToString()), "EXCEPTION | {target} | CacheFlow: ");
        ErrorOccurredInsensitive = LoggerMessage.Define(LogLevel.Error, new EventId((int)CacheEvent.AnErrorHasOccurred, CacheEvent.AnErrorHasOccurred.ToString()), "EXCEPTION | CacheFlow: ");
            
        EntryRemoved = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId((int)CacheEvent.Remove, CacheEvent.Remove.ToString()), "REMOVED | {target} | {Key} | CacheFlow: The key has been removed from a cache.");
        EntryRemovedInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvent.Remove, CacheEvent.Remove.ToString()), "REMOVED | {Key} | CacheFlow: The key has been removed from a cache.");
            
        NoOptions = LoggerMessage.Define<string>(LogLevel.Warning, new EventId((int)CacheEvent.AnErrorHasOccurred, CacheEvent.AnErrorHasOccurred.ToString()), "NO OPTIONS | {target} | CacheFlow: No options has been provided. The defaults are used.");
    }


    internal static void LogHit(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            CacheHit(logger, target, key, Serialize(value), null);
        else
            CacheHitInsensitive(logger, key, null);
    }


    internal static void LogMissed(this ILogger logger, string target, string key, bool logSensitive)
    {
        if (logSensitive)
            CacheMissed(logger, target, key, null);
        else
            CacheMissedInsensitive(logger, key, null);
    }


    internal static void LogNotSet(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            CacheNotSet(logger, target, key, Serialize(value), null);
        else
            CacheNotSetInsensitive(logger, key, null);
    }


    internal static void LogSet(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            CacheSet(logger, target, key, Serialize(value), null);
        else
            CacheSetInsensitive(logger, key, null);
    }


    internal static void LogCacheError(this ILogger logger, string target, Exception exception, bool logSensitive)
    {
        if (logSensitive)
            ErrorOccurred(logger, target, exception);
        else
            ErrorOccurredInsensitive(logger, exception);
    }


    internal static void LogNoOptionsProvided(this ILogger logger, string target)
        => NoOptions(logger, target, null);


    internal static void LogRemoved(this ILogger logger, string target, string key, bool logSensitive)
    {
        if (logSensitive)
            EntryRemoved(logger, target, key, null);
        else
            EntryRemovedInsensitive(logger, key, null);
    }


    private static string Serialize(object target) 
        => JsonSerializer.Serialize(target, JsonSerializerOptions);


    private static readonly Action<ILogger, string, string, string, Exception?> CacheHit;
    private static readonly Action<ILogger, string, Exception?> CacheHitInsensitive;

    private static readonly Action<ILogger, string, string, Exception?> CacheMissed;
    private static readonly Action<ILogger, string, Exception?> CacheMissedInsensitive;

    private static readonly Action<ILogger, string, string, string, Exception?> CacheNotSet;
    private static readonly Action<ILogger, string, Exception?> CacheNotSetInsensitive;

    private static readonly Action<ILogger, string, string, string, Exception?> CacheSet;
    private static readonly Action<ILogger, string, Exception?> CacheSetInsensitive;

    private static readonly Action<ILogger, string, string, Exception?> EntryRemoved;
    private static readonly Action<ILogger, string, Exception?> EntryRemovedInsensitive;

    private static readonly Action<ILogger, string, Exception> ErrorOccurred;
    private static readonly Action<ILogger, Exception> ErrorOccurredInsensitive;

    private static readonly Action<ILogger, string, Exception?> NoOptions;


    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };
}