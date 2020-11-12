using System.Diagnostics;

namespace FloxDc.CacheFlow.Infrastructure
{
    public static class DiagnosticSourceHelper
    {
        internal static Activity GetStartedActivity(this DiagnosticSource source, string activityName, object args = null)
        {
            if (!source.IsEnabled(SourceName))
                return null;

            var current = Activity.Current;
            
            var activity = new Activity(BuildName(activityName));
            if (current != null)
            {
                activity.SetParentId(current.TraceId, current.SpanId, current.ActivityTraceFlags);
                activity.TraceStateString = current.TraceStateString;
            }

            source.StartActivity(activity, args);

            return activity;
        }


        internal static void StopStartedActivity(this DiagnosticSource source, Activity activity, object args = null)
        {
            if (activity is null)
                return;

            source.StopActivity(activity, args);
        }


        private static string BuildName(string activityName) => SourceName + "." + activityName;


        public static readonly string SourceName = $"{nameof(FloxDc)}.{nameof(CacheFlow)}.Cache";
    }
}
