using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow
{
    internal class Executor
    {
        public Executor(ILogger logger, FlowOptions options)
        {
            _logger = logger;
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
                _logger.LogCacheError(ex);
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
                _logger.LogCacheError(ex);
                if (!_options.SuppressCacheExceptions)
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
                if (!_options.SuppressCacheExceptions)
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
                if (!_options.SuppressCacheExceptions)
                    throw;
            }

            return null;
        }


        private readonly ILogger _logger;
        private readonly FlowOptions _options;
    }
}
