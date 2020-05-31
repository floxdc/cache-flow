using FloxDc.CacheFlow.Logging;

namespace FloxDc.CacheFlow.Infrastructure
{
    public readonly struct DiagnosticPayload
    {
        public DiagnosticPayload(CacheEvents cacheEvent, string key, string serviceType)
        {
            Event = cacheEvent;
            Key = key;
            ServiceType = serviceType;
        }


        public CacheEvents Event { get; }
        public string Key { get; }
        public string ServiceType { get; }
    }
}
