using System;
using OpenTelemetry.Adapter;
using OpenTelemetry.Trace;

namespace FloxDc.CacheFlow
{
    public class CacheFlowAdapter : IDisposable
    {
        public CacheFlowAdapter(Tracer tracer, string diagnosticsListenerName)
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
