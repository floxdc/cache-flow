using System;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            CacheHit = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Hit, CacheEvents.Hit.ToString()), "HIT | {Key} | CacheFlow: The cache hit occurs.");
            CacheMiss = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Miss, CacheEvents.Miss.ToString()), "MISS | {Key} | CacheFlow: The cache miss occurs.");
            CacheSkipped = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int) CacheEvents.Skipped, CacheEvents.Skipped.ToString()), "SKIPPED | {Key} | CacheFlow: The key has been skipped due the no-retry policy timeout.");
            ErrorOccured = LoggerMessage.Define(LogLevel.Warning, new EventId((int)CacheEvents.AnErrorHasOccured, CacheEvents.AnErrorHasOccured.ToString()), "EXCEPTION | CacheFlow: ");
            EntryRemoved = LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)CacheEvents.Remove, CacheEvents.Remove.ToString()), "REMOVED | {Key} | CacheFlow: The key has been removed from a cache.");
            NoOptions = LoggerMessage.Define(LogLevel.Warning, new EventId((int)CacheEvents.AnErrorHasOccured, CacheEvents.AnErrorHasOccured.ToString()), "NO OPTIONS | CacheFlow: No options has been provided. The defaults are used.");
        }


        internal static void LogHit(this ILogger logger, string key) 
            => CacheHit(logger, key, null);


        internal static void LogMiss(this ILogger logger, string key) 
            => CacheMiss(logger, key, null);


        internal static void LogNetworkError(this ILogger logger, Exception exception) 
            => ErrorOccured(logger, exception);


        internal static void LogNoOptionsProvided(this ILogger logger)
            => NoOptions(logger, null);


        internal static void LogRemoved(this ILogger logger, string key)
            => EntryRemoved(logger, key, null);


        internal static void LogSkipped(this ILogger logger, string key)
            => CacheSkipped(logger, key, null);


        internal static string Formatter<TState>(TState state, Exception exception)
        {
            if (exception != null)
                return exception.Message;

            if (state != null)
                return state.ToString();

            return "Unable to format message.";
        }


        private static EventId GetEventId<TEnum>(TEnum serviceEvent) where TEnum : Enum
        {
            var name = Enum.GetName(typeof(TEnum), serviceEvent);
            var value = Convert.ToInt32(serviceEvent);

            return new EventId(value, name);
        }


        private static readonly Action<ILogger, string, Exception> CacheHit;
        private static readonly Action<ILogger, string, Exception> CacheMiss;
        private static readonly Action<ILogger, string, Exception> CacheSkipped;
        private static readonly Action<ILogger, string, Exception> EntryRemoved;
        private static readonly Action<ILogger, Exception> ErrorOccured;
        private static readonly Action<ILogger, Exception> NoOptions;
    }
}
