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


        public T Deserialize<T>(object value)
        {
            T result;

            using (var stream = new MemoryStream(value as byte[]))
            {
                stream.Seek(0, SeekOrigin.Begin);
                result = (T) _formatter.Deserialize(stream);
            }
            
            return result;
        }


        public object Serialize<T>(T value)
        {
            object result;

            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, value);
                result = stream.ToArray();
            }

            return result;
        }


        public bool IsBinarySerializer { get; } = IsBinary;
        public bool IsStringSerializer { get; } = !IsBinary;


        private readonly BinaryFormatter _formatter;
        private const bool IsBinary = true;
    }
}
