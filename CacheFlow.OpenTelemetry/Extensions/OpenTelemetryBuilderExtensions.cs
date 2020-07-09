using System;
using FloxDc.CacheFlow.Infrastructure;
using OpenTelemetry.Trace.Configuration;

// ReSharper disable once CheckNamespace
namespace FloxDc.CacheFlow.Extensions
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddCacheFlowInstrumentation(this OpenTelemetryBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.AddInstrumentation(activitySource => new CacheFlowInstrumentation(activitySource, DiagnosticSourceHelper.SourceName));
        }
    }
}
