using System;
using OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow
{
    public class CacheFlowInstrumentation : IDisposable
    {
        /*public CacheFlowInstrumentation(ActivitySourceAdapter activitySource, string diagnosticsListenerName)
        {
            _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(name => new CacheFlowListener(diagnosticsListenerName, activitySource),
                filter => true, (_, __, ___) => true);
            _diagnosticSourceSubscriber.Subscribe();
        }*/


        public void Dispose() 
            => _diagnosticSourceSubscriber?.Dispose();


        private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;
    }
}
