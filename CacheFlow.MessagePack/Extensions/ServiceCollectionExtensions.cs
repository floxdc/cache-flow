using FloxDc.CacheFlow.Serialization;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.MessagePack.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheFlowMessagePackSerialization(this IServiceCollection services, MessagePackSerializerOptions? options = null,
        params IFormatterResolver[]? resolvers)
        => services.AddSingleton<ISerializer, CacheFlowMessagePackSerializer>(serviceProvider => new CacheFlowMessagePackSerializer(options, resolvers));
}