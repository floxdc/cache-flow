using System.Collections.Generic;
using System.Diagnostics;
using FloxDc.CacheFlow.Logging;

namespace FloxDc.CacheFlow.Telemetry;

public static class ActivitySourceHelper
{
    internal static Activity? CreateStartedActivity(this ActivitySource source, string activityName, Dictionary<string, string>? tags = null)
    {
        if (!source.HasListeners())
            return default;

        var activity = Activity.Current is null 
            ? source.StartActivity(BuildName(activityName)) 
            : source.StartActivity(BuildName(activityName), ActivityKind.Internal, Activity.Current.Context);

        if (activity is null)
            return activity;

        if (tags is null)
            return activity;

        foreach (var (key, value) in tags)
            activity.SetTag(key, value);

        return activity;
    }


    internal static void SetEvent(this Activity? target, CacheEvents @event) 
        => target?.SetTag(EventToken, @event.ToString());


    internal const string EventToken = "event";

    private static string BuildName(string activityName) => CacheFlowActivitySourceName + "." + activityName;


    public static readonly string CacheFlowActivitySourceName = $"{nameof(FloxDc)}.{nameof(CacheFlow)}.Cache";
    public const string CacheFlowActivitySourceVersion = "1.0.0";
}