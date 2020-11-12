using FloxDc.CacheFlow.Logging;

namespace FloxDc.CacheFlow.Infrastructure
{
    public readonly struct DiagnosticPayload
    {
        public DiagnosticPayload(CacheEvents cacheEvent, string key, string instanceType)
        {
            Event = cacheEvent;
            InstanceType = instanceType;
            Key = key;
        }


        public CacheEvents Event { get; }
        public string InstanceType { get; }
        public string Key { get; }
    }
}
