using System;
using System.Threading.Tasks;

namespace DAW.CacheFlow
{
    public interface ICacheFlow
    {
        Task<T> GetValueAsync<T>(string key);
        void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
        void SetSliding<T>(string key, T value, TimeSpan slidingExpiration);
        Task SetSlidingAsync<T>(string key, T value, TimeSpan slidingExpiration);
        bool TryGetValue<T>(string key, out T value);
    }
}
