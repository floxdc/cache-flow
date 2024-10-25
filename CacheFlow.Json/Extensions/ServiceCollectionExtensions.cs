using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace CacheFlow.Json.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CacheFlow Newtonsoft JSON serialization services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="serializationOptions">An optional action to configure the JsonSerializerSettings.</param>
    /// <returns>The IServiceCollection with the added services.</returns>
    public static IServiceCollection AddCacheFlowJsonSerialization(this IServiceCollection services, Action<JsonSerializerSettings>? serializationOptions = default)
        => services
            .RegisterOptions(serializationOptions)
            .AddSingleton<ISerializer, CacheFlowJsonSerializer>();


    private static IServiceCollection RegisterOptions(this IServiceCollection services, Action<JsonSerializerSettings>? options)
    {
        if (options is not null)
            services.Configure(options);

        return services;
    }
}