using System;

namespace FloxDc.CacheFlow
{
    public class FlowOptions
    {
        public double DistributedToMemoryExpirationRatio { get; set; } = 10.0;
        public TimeSpan SkipRetryInterval { get; set; } = TimeSpan.FromMinutes(1);
        public bool SuppressCacheExceptions { get; set; } = true;
        public bool UseBinarySerialization { get; set; } = true;
    }
}
