using System;

namespace FloxDc.CacheFlow
{
    public class FlowOptions
    {
        public TimeSpan NoRetryInterval { get; set; } = TimeSpan.FromMinutes(1);
        public bool SuppressCacheExceptions { get; set; } = true;
        public bool UseBinarySerialization { get; set; } = true;
    }
}
