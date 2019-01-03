using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Logging;

namespace FloxDc.CacheFlow.Infrastructure
{
    internal static class Executor
    {
        internal static void Init(ILogger logger, bool suppressCacheExceptions)
        {
            _logger = logger;
            _suppressCacheExceptions = suppressCacheExceptions;
        }


        internal static bool TryExecute(Action action)
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


        internal static T TryExecute<T>(Func<T> func)
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


        internal static async Task<bool> TryExecuteAsync(Func<Task> func)
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


        internal static async Task<object> TryExecuteAsync(Func<Task<object>> func)
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


        private static ILogger _logger;
        private static bool _suppressCacheExceptions;
    }
}
