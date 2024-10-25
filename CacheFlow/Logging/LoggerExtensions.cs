using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Logging;

internal static class LoggerExtensions
{
    internal static void LogHit(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            logger.CacheHit(target, key, Serialize(value));
        else
            logger.CacheHitInsensitive(key);
    }


    internal static void LogMissed(this ILogger logger, string target, string key, bool logSensitive)
    {
        if (logSensitive)
            logger.CacheMissed(target, key);
        else
            logger.CacheMissedInsensitive(key);
    }


    internal static void LogNotSet(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            logger.CacheNotSet(target, key, Serialize(value));
        else
            logger.CacheNotSetInsensitive(key);
    }


    internal static void LogSet(this ILogger logger, string target, string key, object value, bool logSensitive)
    {
        if (logSensitive)
            logger.CacheSet(target, key, Serialize(value));
        else
            logger.CacheSetInsensitive(key);
    }


    internal static void LogCacheError(this ILogger logger, string target, Exception exception, bool logSensitive)
    {
        if (logSensitive)
            logger.ErrorOccurred(target, exception);
        else
            logger.ErrorOccurredInsensitive(exception);
    }


    internal static void LogNoOptionsProvided(this ILogger logger, string target)
        => logger.NoOptions(target);


    internal static void LogRemoved(this ILogger logger, string target, string key, bool logSensitive)
    {
        if (logSensitive)
            logger.EntryRemoved(target, key);
        else
            logger.EntryRemovedInsensitive(key);
    }


    private static string Serialize(object target)
        => JsonSerializer.Serialize(target, JsonSerializerOptions);


    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };
}
