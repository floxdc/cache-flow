using System;

namespace FloxDc.CacheFlow.Infrastructure
{
    public class TextJsonSerializer : ISerializer
    {
        public T Deserialize<T>(string value) 
            => System.Text.Json.JsonSerializer.Deserialize<T>(value);


        public T Deserialize<T>(in ReadOnlyMemory<byte> value) 
            => System.Text.Json.JsonSerializer.Deserialize<T>(value.Span);


        public byte[] Serialize<T>(T value) 
            => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);


        public bool IsBinarySerializer { get; } = IsBinary;
        public bool IsStringSerializer { get; } = !IsBinary;


        private const bool IsBinary = true;
    }
}
