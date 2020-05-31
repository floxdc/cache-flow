using FloxDc.CacheFlow.Infrastructure;
using OpenTelemetry.Trace.Configuration;

namespace FloxDc.CacheFlow.Extensions
{
    public static class TracerBuilderExtensions
    {
        public static TracerBuilder AddCacheFlowAdapter(this TracerBuilder builder) 
            => builder.AddAdapter(tracer => new CacheFlowAdapter(tracer, DiagnosticSourceHelper.SourceName));
    }
}
