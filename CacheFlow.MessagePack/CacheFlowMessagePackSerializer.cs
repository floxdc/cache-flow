using FloxDc.CacheFlow.Infrastructure;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;

namespace CacheFlow.MessagePack
{
    public class CacheFlowMessagePackSerializer : ISerializer
    {
        public CacheFlowMessagePackSerializer()
        {
            CompositeResolver.Create(ImmutableCollectionResolver.Instance, StandardResolver.Instance);
        }


        public T Deserialize<T>(object value) 
            => MessagePackSerializer.Deserialize<T>(value as byte[]);


        public object Serialize<T>(T value) 
            => MessagePackSerializer.Serialize(value);


        public bool IsBinarySerializer { get; } = IsBinary;
        public bool IsStringSerializer { get; } = !IsBinary;


        private const bool IsBinary = true;
    }
}
