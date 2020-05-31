using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow
{
    public class DoubleFlow : IDoubleFlow
    {
        public DoubleFlow(IDistributedFlow distributed, IMemoryFlow memory, ILogger<DoubleFlow> logger = default, IOptions<FlowOptions> options = default)
        {
            _distributed = distributed;
            _memory = memory;

            if (options is null)
            {
                logger?.LogNoOptionsProvided(nameof(DoubleFlow));
                Options = new FlowOptions();
            }
            else
            {
                Options = options.Value;
            }
        }


        public async ValueTask<T> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default)
        {
            if (_memory.TryGetValue(key, out T value))
                return value;

            value = await _distributed.GetAsync<T>(key, cancellationToken);
            if (value is null || value.Equals(default))
                return default;

            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => GetOrSet(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow}
            );
        


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null)
        {
            if (_memory.TryGetValue(key, out T value))
            {
                _distributed.Refresh(key);
                return value;
            }

            value = _distributed.GetOrSet(key, getFunction, distributedOptions);
            
            memoryOptions ??= GetDefaultMemoryOptions(distributedOptions);
            _memory.Set(key, value, memoryOptions);

            return value;
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                cancellationToken
            );


        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (_memory.TryGetValue(key, out T value))
            {
                await _distributed.RefreshAsync(key, cancellationToken);
                return value;
            }

            value = await _distributed.GetOrSetAsync(key, getFunction, distributedOptions, cancellationToken);
            
            memoryOptions ??= GetDefaultMemoryOptions(distributedOptions);
            _memory.Set(key, value, memoryOptions);

            return value;
        }


        public void Refresh(string key) 
            => _distributed.Refresh(key);


        public Task RefreshAsync(string key, CancellationToken cancellationToken = default) 
            => _distributed.RefreshAsync(key, cancellationToken);


        public void Remove(string key)
        {
            _memory.Remove(key);
            _distributed.Remove(key);
        }


        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memory.Remove(key);
            return _distributed.RemoveAsync(key, cancellationToken);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => Set(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow}
            );


        public void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null)
        {
            memoryOptions ??= GetDefaultMemoryOptions(distributedOptions);
            _memory.Set(key, value, memoryOptions);
            _distributed.Set(key, value, distributedOptions);
        }


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => SetAsync(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                cancellationToken
            );


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions,
            MemoryCacheEntryOptions memoryOptions = null,
            CancellationToken cancellationToken = default)
        {
            memoryOptions ??= GetDefaultMemoryOptions(distributedOptions);
            _memory.Set(key, value, memoryOptions);
            return _distributed.SetAsync(key, value, distributedOptions, cancellationToken);
        }


        public bool TryGetValue<T>(string key, out T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => TryGetValue(key, out value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow
                });


        public bool TryGetValue<T>(string key, out T value, MemoryCacheEntryOptions memoryOptions) 
            => _memory.TryGetValue(key, out value) || _distributed.TryGetValue(key, out value);


        private static MemoryCacheEntryOptions GetDefaultMemoryOptions(DistributedCacheEntryOptions distributedOptions)
            => new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = distributedOptions.AbsoluteExpirationRelativeToNow
            };


        public IDistributedCache DistributedInstance => _distributed.Instance;
        public IMemoryCache MemoryInstance => _memory.Instance;
        public FlowOptions Options { get; }


        private readonly IDistributedFlow _distributed;
        private readonly IMemoryFlow _memory;
    }
}
