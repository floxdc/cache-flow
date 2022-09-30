using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace CacheFlow.Json.Extensions;

public static class ServiceCollectionExtensions
{
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