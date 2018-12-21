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
        public DoubleFlow(IDistributedFlow distributed, IMemoryFlow memory, ILogger<DoubleFlow> logger, IOptionsSnapshot<FlowOptions> options)
        {
            _distributed = distributed;
            _memory = memory;

            if (options is null)
            {
                logger?.LogNoOptionsProvided();
                _options = new FlowOptions();
            }
            else
            {
                _options = options.Value;
            }
        }


        public async ValueTask<T> GetAsync<T>(string key, TimeSpan absoluteDistributedExpirationRelativeToNow, CancellationToken cancellationToken = default)
        {
            if (_memory.TryGetValue(key, out T value))
                return value;

            value = await _distributed.GetAsync<T>(key, cancellationToken);
            if (value.Equals(default))
                return default;

            _memory.Set(key, value, GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow));
            return value;
        }


        public T GetOrSet<T>(string key, Func<T> getFunction, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => GetOrSet(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow)}
            );
        


        public T GetOrSet<T>(string key, Func<T> getFunction, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null)
        {
            if (_memory.TryGetValue(key, out T value))
            {
                _distributed.Refresh(key);
                return value;
            }

            if (memoryOptions is null)
                memoryOptions = GetDefaultMemoryOptions(distributedOptions);

            value = _distributed.GetOrSet(key, getFunction, distributedOptions);
            _memory.Set(key, value, memoryOptions);

            return value;
        }


        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction,
            TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, getFunction,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow)},
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

            if (memoryOptions is null)
                memoryOptions = GetDefaultMemoryOptions(distributedOptions);

            value = await _distributed.GetOrSetAsync(key, getFunction, distributedOptions, cancellationToken);
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
            return _distributed.RefreshAsync(key, cancellationToken);
        }


        public void Set<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => Set(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow)}
            );


        public void Set<T>(string key, T value, DistributedCacheEntryOptions distributedOptions, MemoryCacheEntryOptions memoryOptions = null)
        {
            if (memoryOptions is null)
                memoryOptions = GetDefaultMemoryOptions(distributedOptions);

            _memory.Set(key, value, memoryOptions);
            _distributed.Set(key, value, distributedOptions);
        }


        public Task SetAsync<T>(string key, T value, TimeSpan absoluteDistributedExpirationRelativeToNow,
            CancellationToken cancellationToken = default)
            => SetAsync(key, value,
                new DistributedCacheEntryOptions {AbsoluteExpirationRelativeToNow = absoluteDistributedExpirationRelativeToNow},
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow)},
                cancellationToken
            );


        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions distributedOptions,
            MemoryCacheEntryOptions memoryOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (memoryOptions is null)
                memoryOptions = GetDefaultMemoryOptions(distributedOptions);

            _memory.Set(key, value, memoryOptions);
            return _distributed.SetAsync(key, value, distributedOptions, cancellationToken);
        }


        public bool TryGetValue<T>(string key, out T value, TimeSpan absoluteDistributedExpirationRelativeToNow)
            => TryGetValue(key, out value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        GetMemoryExpirationTime(absoluteDistributedExpirationRelativeToNow)
                });


        public bool TryGetValue<T>(string key, out T value, MemoryCacheEntryOptions memoryOptions)
        {
            if (_memory.TryGetValue(key, out value))
                return true;

            if (!_distributed.TryGetValue(key, out value))
                return false;

            _memory.Set(key, value, memoryOptions);
            return true;
        }


        private MemoryCacheEntryOptions GetDefaultMemoryOptions(DistributedCacheEntryOptions distributedOptions)
            => new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    GetMemoryExpirationTime(distributedOptions.AbsoluteExpirationRelativeToNow)
            };


        private TimeSpan GetMemoryExpirationTime(TimeSpan? distributedExpirationTime) 
            => distributedExpirationTime?.Divide(_options.DistributedToMemoryExpirationRatio) ?? TimeSpan.Zero;


        public IDistributedCache DistributedInstance => _distributed.Instance;
        public IMemoryCache MemoryInstance => _memory.Instance;


        private readonly IDistributedFlow _distributed;
        private readonly IMemoryFlow _memory;
        private readonly FlowOptions _options;
    }
}
