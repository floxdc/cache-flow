using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Infrastructure
{
    internal class Executor
    {
        internal Executor(ILogger logger, bool suppressCacheExceptions)
        {
            _logger = logger;
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
                _logger.LogCacheError(ex);
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
                _logger.LogCacheError(ex);
                if (!_suppressCacheExceptions)
                    throw;
            }

            return default;
        }


        internal async Task<bool> TryExecuteAsync(Func<Task> func)
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
                _logger.LogCacheError(ex);
                if (!_suppressCacheExceptions)
                    throw;
            }

            return false;
        }


        internal async Task<object> TryExecuteAsync(Func<Task<object>> func)
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
                _logger.LogCacheError(ex);
                if (!_suppressCacheExceptions)
                    throw;
            }

            return null;
        }


        private readonly ILogger _logger;
        private readonly bool _suppressCacheExceptions;
    }
}
