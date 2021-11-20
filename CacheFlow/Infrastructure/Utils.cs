using System;

namespace FloxDc.CacheFlow.Infrastructure
{
    internal static class Utils
    {
        internal static bool IsDefaultStruct<T>(T value)
            => IsUserDefinedStruct(typeof(T)) && value!.Equals(default(T));


        private static bool IsUserDefinedStruct(Type type) 
            => type.IsValueType && !type.IsEnum && !type.IsPrimitive;
    }
}
