using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FloxDc.CacheFlow.Infrastructure;

internal sealed class Executor
{
    internal Executor(ILogger logger, FlowOptions options)
    {
        _logger = logger;
        _logSensitiveData = options.DataLoggingLevel is DataLogLevel.Sensitive;
        _options = options;
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
            if (!_options.SuppressCacheExceptions)
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
            if (!_options.SuppressCacheExceptions)
                throw;
        }

        return default!;
    }


    internal async ValueTask<bool> TryExecuteAsync(string key, Func<ValueTask> func)
    {
        try
        {
            return await GetOrCreateTaskAsync(key, async () => {
                await func();
                return true;
            });
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecuteAsync)), ex, _logSensitiveData);
            if (!_options.SuppressCacheExceptions)
                throw;
        }

        return false;
    }


    internal async ValueTask<T> TryExecuteAsync<T>(string key, Func<ValueTask<T>> func)
    {
        try
        {
            return await GetOrCreateTaskAsync(key, func);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogCacheError(GetExceptionTarget(nameof(TryExecuteAsync)), ex, _logSensitiveData);
            if (!_options.SuppressCacheExceptions)
                throw;
        }

        return default!;
    }


    private async ValueTask<T> GetOrCreateTaskAsync<T>(string key, Func<ValueTask<T>> func)
    {
        var taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var newEntry = new TaskEntry<T>(taskSource, new CancellationTokenSource());

        if (_pendingTasks.TryAdd(key, newEntry))
        {
            try
            {
                var result = await func();
                taskSource.TrySetResult(result);
                return result;
            }
            catch (Exception ex)
            {
                taskSource.TrySetException(ex);
                throw;
            }
            finally
            {
                // Only remove if it's still our entry (identity check prevents removing a newer entry).
                _pendingTasks.TryRemove(key, out var current);
                if (!ReferenceEquals(current, newEntry))
                    _pendingTasks.TryAdd(key, current!);

                newEntry.CancellationSource.Dispose();
            }
        }

        newEntry.CancellationSource.Dispose();

        _pendingTasks.TryGetValue(key, out var existing);
        if (existing is not TaskEntry<T> taskEntry)
        {
            if (existing is not null)
            {
                var inUseException = new InvalidOperationException($"Cache key '{key}' is already in use with a different value type.");
                _logger.LogCacheError(GetExceptionTarget(nameof(GetOrCreateTaskAsync)), inUseException, _logSensitiveData);
            }

            return await func();
        }

        var joinedTask = taskEntry.TaskSource.Task;
        var elapsed = DateTimeOffset.UtcNow - taskEntry.CreatedAt;
        var remaining = _options.ThunderingHerdProtectionTimeout - elapsed;

        if (remaining <= TimeSpan.Zero)
        {
            var timeoutException = new TimeoutException($"Waiting for cached value timed out after {_options.ThunderingHerdProtectionTimeout.TotalSeconds} seconds");
            _logger.LogCacheError(GetExceptionTarget(nameof(GetOrCreateTaskAsync)), timeoutException, _logSensitiveData);
            return await func();
        }

        if (await Task.WhenAny(joinedTask, Task.Delay(remaining)) == joinedTask)
            return await joinedTask;

        var exception = new TimeoutException($"Waiting for cached value timed out after {_options.ThunderingHerdProtectionTimeout.TotalSeconds} seconds");
        _logger.LogCacheError(GetExceptionTarget(nameof(GetOrCreateTaskAsync)), exception, _logSensitiveData);

        return await func();
    }


    private static string GetExceptionTarget(string methodName) 
        => nameof(Executor) + "::" + methodName;


    private readonly ConcurrentDictionary<string, ITaskEntry> _pendingTasks = new();


    private readonly ILogger _logger;
    private readonly bool _logSensitiveData;
    private readonly FlowOptions _options;
}
