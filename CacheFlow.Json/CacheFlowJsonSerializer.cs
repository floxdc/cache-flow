using FloxDc.CacheFlow.Infrastructure;
using Newtonsoft.Json;

namespace CacheFlow.Json
{
    public class CacheFlowJsonSerializer : ISerializer
    {
        public T Deserialize<T>(object value) 
            => JsonConvert.DeserializeObject<T>(value as string);


        public object Serialize<T>(T value) 
            => JsonConvert.SerializeObject(value);
    }
}
