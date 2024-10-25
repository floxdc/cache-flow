namespace FloxDc.CacheFlow;

/// <summary>
/// Represents CacheFlow options.
/// </summary>
public class FlowOptions
{
    /// <summary>
    /// Delimiter used in cache keys.
    /// </summary>
    public string CacheKeyDelimiter { get; set; } = "::";

    /// <summary>
    /// Prefix used in cache keys.
    /// </summary>
    public string CacheKeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Sets logging of cache values and execution points. Default: Normal.
    /// </summary>
    public DataLogLevel DataLoggingLevel { get; set; } = DataLogLevel.Normal;

    /// <summary>
    /// Enables suppression of exceptions caused by the caching service itself. Default: true.
    /// </summary>
    public bool SuppressCacheExceptions { get; set; } = true;
}
