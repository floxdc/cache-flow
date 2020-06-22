using System;
using OpenTelemetry.Instrumentation;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow
{
    public class CacheFlowInstrumentation : IDisposable
    {
        public CacheFlowInstrumentation(Tracer tracer, string diagnosticsListenerName)
        {
            _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(name => new CacheFlowListener(diagnosticsListenerName, tracer),
                filter => true, (_, __, ___) => true);
            _diagnosticSourceSubscriber.Subscribe();
        }


        public void Dispose() 
            => _diagnosticSourceSubscriber?.Dispose();


        private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;
    }
}
