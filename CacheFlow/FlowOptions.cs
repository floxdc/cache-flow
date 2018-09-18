using System;

namespace FloxDc.CacheFlow
{
    public class FlowOptions
    {
        public TimeSpan InactivityInterval { get; set; } = TimeSpan.FromMinutes(1);
        public int RetryCount { get; set; } = 3;
        public bool SuppressCacheExceptions { get; set; } = true;
        public bool UseBinarySerialization { get; set; } = true;
    }
}
