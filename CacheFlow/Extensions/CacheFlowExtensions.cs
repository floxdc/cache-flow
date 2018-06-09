using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        public static IServiceCollection UseCacheFlow(this IServiceCollection services)
        {
            services.AddSingleton<ICacheFlow, RedisFlow>();

            return services;
        }
    }
}
