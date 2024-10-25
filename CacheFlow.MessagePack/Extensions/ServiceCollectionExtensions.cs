using FloxDc.CacheFlow.Serialization;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.MessagePack.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CacheFlow MessagePack serialization services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="options">Optional MessagePackSerializerOptions to configure the serializer.</param>
    /// <param name="resolvers">Optional array of IFormatterResolver to use for serialization.</param>
    /// <returns>The IServiceCollection with the added services.</returns>
    public static IServiceCollection AddCacheFlowMessagePackSerialization(this IServiceCollection services, MessagePackSerializerOptions? options = null,
        params IFormatterResolver[]? resolvers)
        => services.AddSingleton<ISerializer, CacheFlowMessagePackSerializer>(_ => new CacheFlowMessagePackSerializer(options, resolvers));
}