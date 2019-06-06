using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Infrastructure;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class MemoryFlow : IMemoryFlow
    {
        public MemoryFlow(IMemoryCache memoryCache, ILogger<MemoryFlow> logger = default, IOptions<FlowOptions> options = default)
        {
            Instance = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? new NullLogger<MemoryFlow>();

            FlowOptions internalOptions;
            if (options is null)
            {
                _logger.LogNoOptionsProvided();
                internalOptions = new FlowOptions();
            }
            else
            {
                internalOptions = options.Value;
            }

            _executor = new Executor(_logger, internalOptions.SuppressCacheExceptions);
            _prefix = CacheKeyHelper.GetFullCacheKeyPrefix(internalOptions.CacheKeyPrefix, internalOptions.CacheKeyDelimiter);

            Options = internalOptions;
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


        public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpirationRelativeToNow,
            CancellationToken cancellationToken = default) 
            => GetOrSetAsync(key, getValueFunction, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction,
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
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            _executor.TryExecute(() => Instance.Remove(fullKey));
            _logger.LogRemoved(fullKey);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
            => Set(key, value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        {
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);
            if (Utils.IsDefaultStruct(value))
            {
                _logger.LogNotSetted(fullKey);
                return;
            }

            _executor.TryExecute(() =>
            {
                using (var entry = Instance.CreateEntry(fullKey))
                {
                    entry.SetOptions(options);
                    entry.Value = value;
                }

                _logger.LogSetted(fullKey);
            });
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            var fullKey = CacheKeyHelper.GetFullKey(_prefix, key);

            bool isCached;
            (isCached, value) = _executor.TryExecute(() =>
            {
                var result = Instance.TryGetValue(fullKey, out T obj);
                return (result, obj);
            });

            if (!isCached)
            {
                _logger.LogMissed(fullKey);
                return false;
            }

            _logger.LogHitted(fullKey);
            return true;
        }


        public IMemoryCache Instance { get; }
        public FlowOptions Options { get; }


        private readonly Executor _executor;
        private readonly ILogger<MemoryFlow> _logger;
        private readonly string _prefix;
    }
}
