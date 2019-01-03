using FloxDc.CacheFlow.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFlow.MessagePack.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCashFlowMessagePackSerialization(this IServiceCollection services) 
            => services.AddSingleton<ISerializer, CacheFlowMessagePackSerializer>();
    }
}
