namespace FloxDc.CacheFlow.Infrastructure
{
    public interface ISerializer
    {
        T Deserialize<T>(object value);
        object Serialize<T>(T value);
    }
}
