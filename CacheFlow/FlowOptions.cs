using System;

namespace FloxDc.CacheFlow
{
    /// <summary>
    /// Represents CacheFlow options.
    /// </summary>
    public class FlowOptions
    {
        /// <summary>
        /// The ratio between caching times in a memory and in a distributed cache when DoubleFlow is used.
        /// </summary>
        public double DistributedToMemoryExpirationRatio { get; set; } = 10.0;

        /// <summary>
        /// 
        /// </summary>
        public string CacheKeyDelimiter { get; set; } = "::";

        /// <summary>
        /// 
        /// </summary>
        public string CacheKeyPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Cache will be skipped for that time span if any error occurs in the request process.
        /// </summary>
        public TimeSpan SkipRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Enables suppression of throwing exceptions, caused by caching service itself. Default: true.
        /// </summary>
        public bool SuppressCacheExceptions { get; set; } = true;
    }
}
