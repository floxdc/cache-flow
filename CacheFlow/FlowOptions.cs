using System;

namespace FloxDc.CacheFlow
{
    /// <summary>
    /// Represents CacheFlow options.
    /// </summary>
    public class FlowOptions
    {
        /// <summary>
        /// Cache will be skipped for that time span if any error occurs in the request process.
        /// </summary>
        public TimeSpan SkipRetryInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Enables suppression of throwing exceptions, caused by caching service itself. Default: true.
        /// </summary>
        public bool SuppressCacheExceptions { get; set; } = true;

        /// <summary>
        /// Enables binary serialization to MsgPack format. Overwise, data will be serialized to JSON. Default: true.
        /// </summary>
        public bool UseBinarySerialization { get; set; } = true;
    }
}
