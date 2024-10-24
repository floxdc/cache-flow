using System.Collections.Generic;
using FloxDc.CacheFlow.Logging;
using FloxDc.CacheFlow.Telemetry;

namespace FloxDc.CacheFlow;

public class FlowBase
{
    protected static Dictionary<string, string> BuildTags(CacheEvent @event, string key)
        => new()
        {
            {ActivitySourceHelper.EventToken, @event.ToString()},
            {"key", key},
            {"service-type", nameof(DistributedFlow)}
        };


    protected static string BuildTarget(string className, string methodName) 
        => className + "::" + methodName;


    protected static string GetFullKey(string keyPrefix, string key) 
        => string.Concat(keyPrefix, key);


    protected static string GetFullCacheKeyPrefix(string typeName, string delimiter) 
        => string.Concat(typeName, delimiter);
}