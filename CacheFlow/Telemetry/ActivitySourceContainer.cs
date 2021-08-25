using System.Diagnostics;

namespace FloxDc.CacheFlow.Telemetry
{
    internal static class ActivitySourceContainer
    {
        internal static ActivitySource Instance = new(ActivitySourceHelper.ActivitySourceName, ActivitySourceHelper.ActivitySourceVersion);
    }
}
