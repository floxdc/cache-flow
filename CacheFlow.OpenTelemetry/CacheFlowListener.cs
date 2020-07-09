using System.Diagnostics;
using FloxDc.CacheFlow.Infrastructure;
using OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow
{
    public class CacheFlowListener : ListenerHandler
    {
        public CacheFlowListener(string sourceName, ActivitySourceAdapter activitySource) : base(sourceName, null)
        {
            _activitySource = activitySource;
        }


        public override void OnStartActivity(Activity activity, object payload)
        {
            if (activity is null)
            {
                InstrumentationEventSource.Log.NullActivity(DiagnosticSourceHelper.SourceName);
                return;
            }

            if (!activity.OperationName.StartsWith(SourceName))
                return;
            
            _activitySource.Start(activity);
        }


        public override void OnStopActivity(Activity activity, object payload)
        {
            if (activity is null)
            {
                InstrumentationEventSource.Log.NullActivity(DiagnosticSourceHelper.SourceName);
                return;
            }

            if (!activity.OperationName.StartsWith(SourceName))
                return;
            
            if (activity.IsAllDataRequested && payload is DiagnosticPayload data)
            {
                activity.AddTag(CacheEventAttribute, data.Event.ToString());
                activity.AddTag(CacheKeyAttribute, data.Key);
            }

            _activitySource.Stop(activity);
        }


        private static readonly string CacheEventAttribute = DiagnosticSourceHelper.SourceName + '.' + "event";
        private static readonly string CacheKeyAttribute = DiagnosticSourceHelper.SourceName + '.' + "key";


        private readonly ActivitySourceAdapter _activitySource;
    }
}
