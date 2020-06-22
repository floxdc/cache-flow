using System;
using System.Diagnostics;
using FloxDc.CacheFlow.Infrastructure;
using OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow
{
    public class CacheFlowListener : ListenerHandler
    {
        public CacheFlowListener(string sourceName, Tracer tracer) : base(sourceName, tracer)
        { }


        public override void OnStartActivity(Activity activity, object payload)
        {
            if (!activity.OperationName.StartsWith(SourceName))
                return;

            var span = Tracer.StartSpanFromActivity(activity.OperationName, activity);
            if (!span.IsRecording)
                return;

            foreach (var (key, value) in activity.Tags)
                span.SetAttribute(key, value);
        }


        public override void OnStopActivity(Activity activity, object payload)
        {
            if (!activity.OperationName.StartsWith(SourceName))
                return;
            
            var span = Tracer.CurrentSpan;

            if (span.IsRecording && payload is DiagnosticPayload data)
            {
                span.SetAttribute(CacheEventAttribute, data.Event.ToString());
                span.SetAttribute(CacheKeyAttribute, data.Key);
            }

            span.End();
            if (span is IDisposable disposable)
                disposable.Dispose();
        }


        private static readonly string CacheEventAttribute = DiagnosticSourceHelper.SourceName + '.' + "event";
        private static readonly string CacheKeyAttribute = DiagnosticSourceHelper.SourceName + '.' + "key";
    }
}
