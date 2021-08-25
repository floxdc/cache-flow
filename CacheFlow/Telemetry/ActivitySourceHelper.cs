using System.Collections.Generic;
using System.Diagnostics;

namespace FloxDc.CacheFlow.Telemetry
{
    public static class ActivitySourceHelper
    {
        internal static Activity? GetStartedActivity(this ActivitySource source, string activityName)
        {
            if (!source.HasListeners())
                return default;

            return Activity.Current is null 
                ? source.StartActivity(BuildName(activityName)) 
                : source.StartActivity(BuildName(activityName), ActivityKind.Internal, Activity.Current.Context);
        }


        internal static void StopStartedActivity(this ActivitySource _, Activity? activity, Dictionary<string, string>? tags = null)
        {
            if (activity is null)
                return;

            if (tags is not null)
            {
                foreach (var (key, value) in tags)
                    activity.SetTag(key, value);
            }

            activity.Stop();
        }


        private static string BuildName(string activityName) => ActivitySourceName + "." + activityName;


        public static readonly string ActivitySourceName = $"{nameof(FloxDc)}.{nameof(CacheFlow)}.Cache";
        public const string ActivitySourceVersion = "1.0.0";
    }
}
