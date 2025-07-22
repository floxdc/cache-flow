using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Infrastructure;

internal sealed class Executor
{
    internal Executor(ILogger logger, bool suppressCacheExceptions, DataLogLevel dataLogLevel)
    {
        _logger = logger;
        _logSensitiveData = dataLogLevel is DataLogLevel.Sensitive;
        _suppressCacheExceptions = suppressCacheExceptions;
    }


    internal bool TryExecute(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecute)), ex, _logSensitiveData);
            if (!_suppressCacheExceptions)
                throw;
        }

        return false;
    }


    internal T TryExecute<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecute)), ex, _logSensitiveData);
            if (!_suppressCacheExceptions)
                throw;
        }

        return default!;
    }


    internal async ValueTask<bool> TryExecuteAsync(Func<ValueTask> func)
    {
        try
        {
            await func();
            return true;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecuteAsync)), ex, _logSensitiveData);
            if (!_suppressCacheExceptions)
                throw;
        }

        return false;
    }


    internal async ValueTask<T> TryExecuteAsync<T>(Func<ValueTask<T>> func)
    {
        try
        {
            return await func();
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecuteAsync)), ex, _logSensitiveData);
            if (!_suppressCacheExceptions)
                throw;
        }

        return default!;
    }


    private static string GetExceptionTarget(string methodName) 
        => nameof(Executor) + "::" + methodName;


    private readonly ILogger _logger;
    private readonly bool _logSensitiveData;
    private readonly bool _suppressCacheExceptions;
}