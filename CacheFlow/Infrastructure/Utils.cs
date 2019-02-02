using System;

namespace FloxDc.CacheFlow.Infrastructure
{
    internal static class Utils
    {
        internal static bool IsDefaultStruct<T>(T value)
            => IsStruct(typeof(T)) && value.Equals(default(T));


        private static bool IsStruct(Type type) 
            => type.IsValueType && !type.IsEnum;
    }
}
