using Microsoft.Extensions.DependencyInjection;

namespace FloxDc.CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        /// <summary>
        /// Use CacheFlow with a distributed cache provider.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCacheFlow(this IServiceCollection services)
        {
            services.AddDistributedFlow();
            services.AddMemoryFlow();

            return services;
        }


        /// <summary>
        /// Use CacheFlow with a distributed cache provider.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedFlow(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedFlow, DistributedFlow>();

            return services;
        }


        public static IServiceCollection AddMemoryFlow(this IServiceCollection services)
        {
            services.AddSingleton<IMemoryFlow, MemoryFlow>();

            return services;
        }
    }
}
