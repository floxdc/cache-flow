using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            CacheHit = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int)CacheEvents.Hit, CacheEvents.Hit.ToString()), "HIT | {target} | {Key} | {value} | CacheFlow: The cache hit occurs.");
            CacheHitInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvents.Hit, CacheEvents.Hit.ToString()), "HIT | {Key} | CacheFlow: The cache hit occurs.");
            
            CacheMissed = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId((int)CacheEvents.Miss, CacheEvents.Miss.ToString()), "MISS | {target} | {Key} | CacheFlow: The cache miss occurs.");
            CacheMissedInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvents.Miss, CacheEvents.Miss.ToString()), "MISS | {Key} | CacheFlow: The cache miss occurs.");
            
            CacheNotSet = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int) CacheEvents.Skipped, CacheEvents.Skipped.ToString()), "SKIPPED | {target} | {Key} | {value} | CacheFlow: The key has not been set, because the entry is a default struct value.");
            CacheNotSetInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int) CacheEvents.Skipped, CacheEvents.Skipped.ToString()), "SKIPPED | {Key} | CacheFlow: The key has not been set, because the entry is a default struct value.");
            
            CacheSet = LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId((int)CacheEvents.Set, CacheEvents.Set.ToString()), "SET | {target} | {Key} | {value} | CacheFlow: The entry is set.");
            CacheSetInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvents.Set, CacheEvents.Set.ToString()), "SET | {Key} | CacheFlow: The entry is set.");
            
            ErrorOccured = LoggerMessage.Define<string>(LogLevel.Error, new EventId((int)CacheEvents.AnErrorHasOccurred, CacheEvents.AnErrorHasOccurred.ToString()), "EXCEPTION | {target} | CacheFlow: ");
            ErrorOccuredInsensitive = LoggerMessage.Define(LogLevel.Error, new EventId((int)CacheEvents.AnErrorHasOccurred, CacheEvents.AnErrorHasOccurred.ToString()), "EXCEPTION | CacheFlow: ");
            
            EntryRemoved = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId((int)CacheEvents.Remove, CacheEvents.Remove.ToString()), "REMOVED | {target} | {Key} | CacheFlow: The key has been removed from a cache.");
            EntryRemovedInsensitive = LoggerMessage.Define<string>(LogLevel.Debug, new EventId((int)CacheEvents.Remove, CacheEvents.Remove.ToString()), "REMOVED | {Key} | CacheFlow: The key has been removed from a cache.");
            
            NoOptions = LoggerMessage.Define<string>(LogLevel.Warning, new EventId((int)CacheEvents.AnErrorHasOccurred, CacheEvents.AnErrorHasOccurred.ToString()), "NO OPTIONS | {target} | CacheFlow: No options has been provided. The defaults are used.");
        }


        internal static void LogHit(this ILogger logger, string target, string key, object value)
        {
            var json = JsonSerializer.Serialize(value, JsonSerializerOptions);
            CacheHit(logger, target, key, json, null);
        }


        internal static void LogHitInsensitive(this ILogger logger, string key) 
            => CacheHitInsensitive(logger, key, null);


        internal static void LogMissed(this ILogger logger, string target, string key) 
            => CacheMissed(logger, target, key, null);


        internal static void LogMissedInsensitive(this ILogger logger, string key) 
            => CacheMissedInsensitive(logger, key, null);


        internal static void LogNotSet(this ILogger logger, string target, string key, object value)
        {
            var json = JsonSerializer.Serialize(value, JsonSerializerOptions);
            CacheNotSet(logger, target, key, json, null);
        }


        internal static void LogNotSetInsensitive(this ILogger logger, string key)
            => CacheNotSetInsensitive(logger, key, null);


        internal static void LogSet(this ILogger logger, string target, string key, object value)
        {
            var json = JsonSerializer.Serialize(value, JsonSerializerOptions);
            CacheSet(logger, target, key, json, null);
        }


        internal static void LogSetInsensitive(this ILogger logger, string key)
            => CacheSetInsensitive(logger, key, null);


        internal static void LogCacheError(this ILogger logger, string target, Exception exception) 
            => ErrorOccured(logger, target, exception);


        internal static void LogCacheErrorInsensitive(this ILogger logger, Exception exception) 
            => ErrorOccuredInsensitive(logger, exception);


        internal static void LogNoOptionsProvided(this ILogger logger, string target)
            => NoOptions(logger, target, null);


        internal static void LogRemoved(this ILogger logger, string target, string key) 
            => EntryRemoved(logger, target, key, null);


        internal static void LogRemovedInsensitive(this ILogger logger, string key)
            => EntryRemovedInsensitive(logger, key, null);


        private static readonly Action<ILogger, string, string, string, Exception> CacheHit;
        private static readonly Action<ILogger, string, Exception> CacheHitInsensitive;

        private static readonly Action<ILogger, string, string, Exception> CacheMissed;
        private static readonly Action<ILogger, string, Exception> CacheMissedInsensitive;

        private static readonly Action<ILogger, string, string, string, Exception> CacheNotSet;
        private static readonly Action<ILogger, string, Exception> CacheNotSetInsensitive;

        private static readonly Action<ILogger, string, string, string, Exception> CacheSet;
        private static readonly Action<ILogger, string, Exception> CacheSetInsensitive;

        private static readonly Action<ILogger, string, string, Exception> EntryRemoved;
        private static readonly Action<ILogger, string, Exception> EntryRemovedInsensitive;

        private static readonly Action<ILogger, string, Exception> ErrorOccured;
        private static readonly Action<ILogger, Exception> ErrorOccuredInsensitive;

        private static readonly Action<ILogger, string, Exception> NoOptions;


        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }
}
