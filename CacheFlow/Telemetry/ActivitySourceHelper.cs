using System.Diagnostics;
using FloxDc.CacheFlow.Logging;

namespace FloxDc.CacheFlow.Telemetry;

public static class ActivitySourceHelper
{
    internal static Activity? CreateStartedActivity(this ActivitySource source, string activityName)
    {
        if (!source.HasListeners())
            return default;

        return Activity.Current is null
            ? source.StartActivity(BuildName(activityName))
            : source.StartActivity(BuildName(activityName), ActivityKind.Internal, Activity.Current.Context);
    }


    internal static Activity? CreateStartedActivity(this ActivitySource source, string activityName, CacheEvent @event, string key)
    {
        if (!source.HasListeners())
            return default;

        var activity = Activity.Current is null
            ? source.StartActivity(BuildName(activityName))
            : source.StartActivity(BuildName(activityName), ActivityKind.Internal, Activity.Current.Context);

        if (activity is null)
            return activity;

        activity.SetTag(EventToken, @event.ToString());
        activity.SetTag("key", key);
        activity.SetTag("service-type", nameof(DistributedFlow));

        return activity;
    }


    internal static void SetEvent(this Activity? target, CacheEvent @event) 
        => target?.SetTag(EventToken, @event.ToString());


    internal const string EventToken = "event";

    private static string BuildName(string activityName) => CacheFlowActivitySourceName + "." + activityName;


    public static readonly string CacheFlowActivitySourceName = $"{nameof(FloxDc)}.{nameof(CacheFlow)}.Cache";
    public const string CacheFlowActivitySourceVersion = "1.0.0";
}