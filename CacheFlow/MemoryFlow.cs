using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class MemoryFlow : IMemoryFlow
    {
        public MemoryFlow(IMemoryCache memoryCache, ILogger<MemoryFlow> logger = default, IOptions<FlowOptions> options = default)
        {
            Instance = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger;

            FlowOptions internalOptions;
            if (options is null)
            {
                _logger?.LogNoOptionsProvided();
                internalOptions = new FlowOptions();
            }
            else
            {
                internalOptions = options.Value;
            }

            _executor = new Executor(_logger, internalOptions.SuppressCacheExceptions);
            _prefix = GetFullCacheKeyPrefix(internalOptions.CacheKeyPrefix, internalOptions.CacheKeyDelimiter);
        }


        public T GetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpirationRelativeToNow)
            => GetOrSet(key, getValueFunction,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public T GetOrSet<T>(string key, Func<T> getValueFunction, MemoryCacheEntryOptions options)
        {
            if (TryGetValue(key, out T result))
                return result;

            result = getValueFunction();
            Set(key, result, options);

            return result;
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default) 
            => GetOrSetAsync(key, getValueFunction, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction,
            MemoryCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            if (TryGetValue(key, out T result))
                return result;

            result = await getValueFunction();
            Set(key, result, options);

            return result;
        }


        public void Remove(string key)
        {
            var fullKey = GetFullKey(_prefix, key);

            _executor.TryExecute(() =>
            {
                Instance.Remove(fullKey);
                _logger?.LogRemoved(fullKey);
            });
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
            => Set(key, value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        {
            var fullKey = GetFullKey(_prefix, key);
            if (Utils.IsDefaultStruct(typeof(T)))
            {
                _logger.LogNotSetted(fullKey);
                return;
            }

            _executor.TryExecute(() =>
            {
                Instance.Set(fullKey, value, options);
                _logger?.LogSetted(fullKey);
            });
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            var fullKey = GetFullKey(_prefix, key);

            value = _executor.TryExecute(() => Instance.Get<T>(fullKey));
            if (value == null)
            {
                _logger?.LogMissed(fullKey);
                return false;
            }

            _logger?.LogHitted(fullKey);
            return true;
        }


        public IMemoryCache Instance { get; }


        private static string GetFullCacheKeyPrefix(string prefix, string delimiter) 
            => string.IsNullOrWhiteSpace(prefix) ? string.Empty : string.Concat(prefix, delimiter);


        private string GetFullKey(string prefix, string key)
            => prefix == string.Empty ? key : string.Concat(_prefix, key);


        private readonly Executor _executor;
        private readonly ILogger<MemoryFlow> _logger;
        private readonly string _prefix;
    }
}
