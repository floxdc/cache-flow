using System.Diagnostics;
using FloxDc.CacheFlow.Logging;

namespace FloxDc.CacheFlow.Infrastructure
{
    public static class DiagnosticSourceHelper
    {
        internal static DiagnosticPayload BuildArguments(CacheEvents @event, string key, string serviceType) 
            => new DiagnosticPayload(@event, key, serviceType);


        internal static Activity GetStartedActivity(this DiagnosticSource source, string activityName, object args = null)
        {
            if (!source.IsEnabled(SourceName))
                return null;

            var activity = new Activity(BuildName(activityName));
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
