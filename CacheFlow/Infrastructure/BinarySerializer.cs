using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FloxDc.CacheFlow.Infrastructure
{
    public class BinarySerializer : ISerializer
    {
        public BinarySerializer()
        {
            _formatter = new BinaryFormatter();
        }


        public T Deserialize<T>(string _) => 
            throw new NotImplementedException("The string overload is disabled for binary formatters");


        public T Deserialize<T>(in ReadOnlyMemory<byte> value)
        {
            using var stream = new MemoryStream(value.ToArray());
            stream.Seek(0, SeekOrigin.Begin);
            var result = (T) _formatter.Deserialize(stream);

            return result;
        }


        public byte[] Serialize<T>(T value)
        {
            using var stream = new MemoryStream();
            _formatter.Serialize(stream, value);
            return stream.ToArray();
        }


        public bool IsBinarySerializer { get; } = IsBinary;
        public bool IsStringSerializer { get; } = !IsBinary;


        private readonly BinaryFormatter _formatter;
        private const bool IsBinary = true;
    }
}
