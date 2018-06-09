using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        public static IServiceCollection AddCacheFlow(this IServiceCollection services)
        {
            services.AddSingleton<ICacheFlow, RedisFlow>();

            return services;
        }
    }
}
