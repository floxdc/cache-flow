using System;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            CacheHitted = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Hit, CacheEvents.Hit.ToString()), "HIT | {Key} | CacheFlow: The cache hit occurs.");
            CacheMissed = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Miss, CacheEvents.Miss.ToString()), "MISS | {Key} | CacheFlow: The cache miss occurs.");
            CacheNotSetted = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int) CacheEvents.Skipped, CacheEvents.Skipped.ToString()), "SKIPPED | {Key} | CacheFlow: The key has not been set, because the entry is a default struct value.");
            CacheSeted = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Set, CacheEvents.Set.ToString()), "SET | {Key} | CacheFlow: The entry is set.");
            CacheSkipped = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int) CacheEvents.Skipped, CacheEvents.Skipped.ToString()), "SKIPPED | {Key} | CacheFlow: The key has been skipped due the no-retry policy timeout.");
            ErrorOccured = LoggerMessage.Define(LogLevel.Warning, new EventId((int)CacheEvents.AnErrorHasOccured, CacheEvents.AnErrorHasOccured.ToString()), "EXCEPTION | CacheFlow: ");
            EntryRemoved = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Remove, CacheEvents.Remove.ToString()), "REMOVED | {Key} | CacheFlow: The key has been removed from a cache.");
            NoOptions = LoggerMessage.Define(LogLevel.Warning, new EventId((int)CacheEvents.AnErrorHasOccured, CacheEvents.AnErrorHasOccured.ToString()), "NO OPTIONS | CacheFlow: No options has been provided. The defaults are used.");
        }


        internal static void LogHitted(this ILogger logger, string key) 
            => CacheHitted(logger, key, null);


        internal static void LogMissed(this ILogger logger, string key) 
            => CacheMissed(logger, key, null);


        internal static void LogNotSetted(this ILogger logger, string key)
            => CacheNotSetted(logger, key, null);


        internal static void LogSetted(this ILogger logger, string key)
            => CacheSeted(logger, key, null);


        internal static void LogCacheError(this ILogger logger, Exception exception) 
            => ErrorOccured(logger, exception);


        internal static void LogNoOptionsProvided(this ILogger logger)
            => NoOptions(logger, null);


        internal static void LogRemoved(this ILogger logger, string key)
            => EntryRemoved(logger, key, null);


        internal static void LogSkipped(this ILogger logger, string key)
            => CacheSkipped(logger, key, null);



        private static readonly Action<ILogger, string, Exception> CacheHitted;
        private static readonly Action<ILogger, string, Exception> CacheMissed;
        private static readonly Action<ILogger, string, Exception> CacheNotSetted;
        private static readonly Action<ILogger, string, Exception> CacheSeted;
        private static readonly Action<ILogger, string, Exception> CacheSkipped;
        private static readonly Action<ILogger, string, Exception> EntryRemoved;
        private static readonly Action<ILogger, Exception> ErrorOccured;
        private static readonly Action<ILogger, Exception> NoOptions;
    }
}
