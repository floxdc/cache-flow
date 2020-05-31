using FloxDc.CacheFlow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.Json.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheFlowJsonSerialization(this IServiceCollection services)
            => services.AddSingleton<ISerializer, CacheFlowJsonSerializer>();
    }
}
