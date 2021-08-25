using System;

namespace FloxDc.CacheFlow.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(string value);
        T Deserialize<T>(in ReadOnlyMemory<byte> value);
        byte[] Serialize<T>(T value);

        bool IsBinarySerializer { get; }
        bool IsStringSerializer { get; }
    }
}
