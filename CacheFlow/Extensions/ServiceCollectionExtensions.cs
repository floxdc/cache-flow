using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace FloxDc.CacheFlow.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use CacheFlow with default settings.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="defaultSerializationOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCacheFlow(this IServiceCollection services, Action<FlowOptions>? options = default, Action<JsonSerializerOptions>? defaultSerializationOptions = default)
        => services
            .RegisterOptions(options)
            .RegisterOptions(defaultSerializationOptions)
            .AddDistributedFlowInternal()
            .AddMemoryFlowInternal();


    /// <summary>
    /// Use CacheFlow with a distributed cache provider.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="defaultSerializationOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddDistributedFlow(this IServiceCollection services, Action<FlowOptions>? options = null, Action<JsonSerializerOptions>? defaultSerializationOptions = default)
        => services
            .RegisterOptions(options)
            .RegisterOptions(defaultSerializationOptions)
            .AddDistributedFlowInternal();


    /// <summary>
    /// Use CacheFlow with distributed and in-memory cache providers.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="defaultSerializationOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddDoubleFlow(this IServiceCollection services, Action<FlowOptions>? options = null, Action<JsonSerializerOptions>? defaultSerializationOptions = default)
        => services
            .RegisterOptions(options)
            .RegisterOptions(defaultSerializationOptions)
            .AddDistributedFlowInternal()
            .AddMemoryFlowInternal()
            .AddSingleton<IDoubleFlow, DoubleFlow>()
            .AddSingleton(typeof(IDoubleFlow<>), typeof(DoubleFlow<>));


    /// <summary>
    /// Use CacheFlow with a in-memory cache provider.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="defaultSerializationOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddMemoryFlow(this IServiceCollection services, Action<FlowOptions>? options = null, Action<JsonSerializerOptions>? defaultSerializationOptions = default)
        => services
            .RegisterOptions(options)
            .RegisterOptions(defaultSerializationOptions)
            .AddMemoryFlowInternal();


    private static IServiceCollection AddDistributedFlowInternal(this IServiceCollection services)
        => services
            .AddSingleton<IDistributedFlow, DistributedFlow>()
            .AddSingleton(typeof(IDistributedFlow<>), typeof(DistributedFlow<>));


    private static IServiceCollection AddMemoryFlowInternal(this IServiceCollection services)
        => services
            .AddSingleton<IMemoryFlow, MemoryFlow>()
            .AddSingleton(typeof(IMemoryFlow<>), typeof(MemoryFlow<>));


    private static IServiceCollection RegisterOptions<T>(this IServiceCollection services, Action<T>? options) where T : class
    {
        if (options is not null)
            services.Configure(options);

        return services;
    }
}