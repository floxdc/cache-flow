using FloxDc.CacheFlow.Infrastructure;
using OpenTelemetry.Trace.Configuration;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow.Extensions
{
    public static class TracerBuilderExtensions
    {
        public static TracerBuilder AddCacheFlowInstrumentation(this TracerBuilder builder) 
            => builder.AddInstrumentation(tracer => new CacheFlowInstrumentation(tracer, DiagnosticSourceHelper.SourceName));
    }
}
