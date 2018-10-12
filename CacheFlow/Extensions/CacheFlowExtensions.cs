using Microsoft.Extensions.DependencyInjection;

namespace FloxDc.CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        public static IServiceCollection AddCacheFlow(this IServiceCollection services)
        {
            services.AddDistributedFlow();
            services.AddMemoryFlow();

            return services;
        }


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
