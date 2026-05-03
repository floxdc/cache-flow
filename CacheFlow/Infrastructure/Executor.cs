using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            _logger.LogCacheError(_targetTryExecute, ex, _logSensitiveData);
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
            _logger.LogCacheError(_targetTryExecute, ex, _logSensitiveData);
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
            _logger.LogCacheError(_targetTryExecuteAsync, ex, _logSensitiveData);
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
            _logger.LogCacheError(_targetTryExecuteAsync, ex, _logSensitiveData);
            if (!_options.SuppressCacheExceptions)
                throw;
        }

        return default!;
    }


    private async ValueTask<T> GetOrCreateTaskAsync<T>(string key, Func<ValueTask<T>> func)
    {
        var taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var newEntry = new TaskEntry<T>(taskSource);

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
                // Only remove if it's still our entry.
                ((ICollection<KeyValuePair<string, ITaskEntry>>)_pendingTasks).Remove(new KeyValuePair<string, ITaskEntry>(key, newEntry));
            }
        }

        _pendingTasks.TryGetValue(key, out var existing);
        if (existing is not TaskEntry<T> taskEntry)
        {
            if (existing is not null)
            {
                var inUseException = new InvalidOperationException($"Cache key '{key}' is already in use with a different value type.");
                _logger.LogCacheError(_targetGetOrCreate, inUseException, _logSensitiveData);
            }

            return await func();
        }

        var joinedTask = taskEntry.TaskSource.Task;
        var elapsed = DateTimeOffset.UtcNow - taskEntry.CreatedAt;
        var remaining = _options.ThunderingHerdProtectionTimeout - elapsed;

        if (remaining <= TimeSpan.Zero)
        {
            var timeoutException = new TimeoutException($"Waiting for cached value timed out after {_options.ThunderingHerdProtectionTimeout.TotalSeconds} seconds");
            _logger.LogCacheError(_targetGetOrCreate, timeoutException, _logSensitiveData);
            return await func();
        }

        if (await Task.WhenAny(joinedTask, Task.Delay(remaining)) == joinedTask)
            return await joinedTask;

        var exception = new TimeoutException($"Waiting for cached value timed out after {_options.ThunderingHerdProtectionTimeout.TotalSeconds} seconds");
        _logger.LogCacheError(_targetGetOrCreate, exception, _logSensitiveData);

        return await func();
    }


    private const string _classPrefix = nameof(Executor) + "::";
    private static readonly string _targetTryExecute = _classPrefix + nameof(TryExecute);
    private static readonly string _targetTryExecuteAsync = _classPrefix + nameof(TryExecuteAsync);
    private static readonly string _targetGetOrCreate = _classPrefix + nameof(GetOrCreateTaskAsync);


    private readonly ConcurrentDictionary<string, ITaskEntry> _pendingTasks = new();
    private readonly ILogger _logger;
    private readonly bool _logSensitiveData;
    private readonly FlowOptions _options;
}
