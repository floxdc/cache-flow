using System;
using Microsoft.Extensions.DependencyInjection;

namespace FloxDc.CacheFlow.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Use CacheFlow with default settings.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddCacheFlow(this IServiceCollection services, Action<FlowOptions>? options = null) 
            => services.AddDistributedFlow(options)
                .AddMemoryFlow();


        /// <summary>
        /// Use CacheFlow with a distributed cache provider.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddDistributedFlow(this IServiceCollection services, Action<FlowOptions>? options = null)
            => services.RegisterOptions(options)
                .AddSingleton<IDistributedFlow, DistributedFlow>();


        /// <summary>
        /// Use CacheFlow with distributed and in-memory cache providers.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddDoubleFlow(this IServiceCollection services, Action<FlowOptions>? options = null) 
            => services.AddDistributedFlow(options)
                .AddMemoryFlow()
                .AddSingleton<IDoubleFlow, DoubleFlow>();


        /// <summary>
        /// Use CacheFlow with a in-memory cache provider.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddMemoryFlow(this IServiceCollection services, Action<FlowOptions>? options = null) 
            => services.RegisterOptions(options)
                .AddSingleton<IMemoryFlow, MemoryFlow>()
                .AddSingleton(typeof(IMemoryFlow<>), typeof(MemoryFlow<>));


        private static IServiceCollection RegisterOptions(this IServiceCollection services, Action<FlowOptions>? options)
        {
            if (options != null)
                services.Configure(options);

            return services;
        }
    }
}
