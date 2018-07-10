using Microsoft.Extensions.DependencyInjection;

namespace FloxDc.CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        public static IServiceCollection AddCacheFlow(this IServiceCollection services)
        {
            services.AddSingleton<ICacheFlow, DistributedFlow>();

            return services;
        }


        public static IServiceCollection AddDistributedFlow(this IServiceCollection services)
        {
            services.AddSingleton<ICacheFlow, DistributedFlow>();

            return services;
        }
    }
}
