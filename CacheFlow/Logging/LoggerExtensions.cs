using System;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging
{
    internal static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            CacheHit = LoggerMessage.Define<string>(LogLevel.Information, GetEventId(CacheEvents.Hit), "HIT | {Key} | The cache hit occurs.");
            CacheMiss = LoggerMessage.Define<string>(LogLevel.Information, GetEventId(CacheEvents.Hit), "MISS | {Key} | The cache miss occurs.");
            EntryRemoved = LoggerMessage.Define<string>(LogLevel.Information, GetEventId(CacheEvents.Hit), "REMOVED | {Key} | The key has been removed from a cache.");
            ErrorOccured = LoggerMessage.Define(LogLevel.Warning, GetEventId(CacheEvents.AnErrorHasOccured), "");
        }


        internal static void Hit(this ILogger logger, string key) 
            => CacheHit(logger, key, null);


        internal static void Miss(this ILogger logger, string key) 
            => CacheMiss(logger, key, null);


        internal static void NetworkError(this ILogger logger, Exception exception) 
            => ErrorOccured(logger, exception);


        internal static void Remove(this ILogger logger, string key)
            => EntryRemoved(logger, key, null);


        internal static EventId GetEventId<TEnum>(TEnum serviceEvent) where TEnum : Enum
        {
            var name = Enum.GetName(typeof(TEnum), serviceEvent);
            var value = Convert.ToInt32(serviceEvent);

            return new EventId(value, name);
        }


        internal static string Formatter<TState>(TState state, Exception exception)
        {
            if (exception != null)
                return exception.Message;

            if (state != null)
                return state.ToString();

            return "Unable to format message.";
        }


        private static readonly Action<ILogger, string, Exception> CacheHit;
        private static readonly Action<ILogger, string, Exception> CacheMiss;
        private static readonly Action<ILogger, string, Exception> EntryRemoved;
        private static readonly Action<ILogger, Exception> ErrorOccured;
    }
}
