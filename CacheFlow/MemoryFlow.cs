﻿using System;
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
        public MemoryFlow(IMemoryCache memoryCache, ILogger<MemoryFlow> logger, IOptions<FlowOptions> options)
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


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, MemoryCacheEntryOptions options)
        {
            if (TryGetValue(key, out T result))
                return result;

            result = await getValueFunction();
            Set(key, result, options);

            return result;
        }


        public void Remove(string key)
            => _executor.TryExecute(() =>
            {
                Instance.Remove(key);
                _logger?.LogRemoved(key);
            });


        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) 
            => Set(key, value, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow});


        public void Set<T>(string key, T value, MemoryCacheEntryOptions options)
        {
            if (Utils.IsDefaultStruct(typeof(T)))
            {
                _logger.LogNotSetted(key);
                return;
            }

            _executor.TryExecute(() =>
            {
                Instance.Set(key, value, options);
                _logger?.LogSetted(key);
            });
        }


        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            value = _executor.TryExecute(() => Instance.Get<T>(key));
            if (value == null)
            {
                _logger?.LogMissed(key);
                return false;
            }

            _logger?.LogHitted(key);
            return true;
        }


        public IMemoryCache Instance { get; }

        private readonly Executor _executor;
        private readonly ILogger<MemoryFlow> _logger;
    }
}
