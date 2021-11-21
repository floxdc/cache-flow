using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.Json.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheFlowJsonSerialization(this IServiceCollection services)
        => services.AddSingleton<ISerializer, CacheFlowJsonSerializer>();
}