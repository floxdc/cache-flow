namespace FloxDc.CacheFlow
{
    /// <summary>
    /// Represents CacheFlow options.
    /// </summary>
    public class FlowOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public string CacheKeyDelimiter { get; set; } = "::";

        /// <summary>
        /// 
        /// </summary>
        public string CacheKeyPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Sets logging of cache values and execution points. Default: Normal.
        /// </summary>
        public DataLogLevel DataLoggingLevel { get; set; } = DataLogLevel.Normal;

        /// <summary>
        /// Enables suppression of throwing exceptions, caused by caching service itself. Default: true.
        /// </summary>
        public bool SuppressCacheExceptions { get; set; } = true;
    }
}
