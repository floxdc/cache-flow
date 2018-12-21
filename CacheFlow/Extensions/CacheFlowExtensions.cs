﻿using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow.Extensions
{
    public static class CacheFlowExtensions
    {
        /// <summary>
        /// Use CacheFlow with default settings.
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


        /// <summary>
        /// Use CacheFlow with distributed and in-memory cache providers.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddDoubleFlow(this IServiceCollection services, IServiceProvider provider)
        {
            var options = provider.GetService<IOptionsSnapshot<FlowOptions>>();

            var distributedCache = provider.GetService<IDistributedCache>();
            var distributedLogger = provider.GetService<ILogger<DistributedFlow>>();

            var memoryCache = provider.GetService<IMemoryCache>();
            var memoryLogger = provider.GetService<ILogger<MemoryFlow>>();

            var distributed = new DistributedFlow(distributedCache, distributedLogger, options);
            var memory = new MemoryFlow(memoryCache, memoryLogger, options);

            var doubleLogger = provider.GetService<ILogger<DoubleFlow>>();

            services.AddSingleton<IDoubleFlow>(new DoubleFlow(distributed, memory, doubleLogger, options));

            return services;
        }


        /// <summary>
        /// Use CacheFlow with a in-memory cache provider.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMemoryFlow(this IServiceCollection services)
        {
            services.AddSingleton<IMemoryFlow, MemoryFlow>();

            return services;
        }
    }
}
