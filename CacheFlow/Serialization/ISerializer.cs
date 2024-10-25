using System;

namespace FloxDc.CacheFlow.Serialization;

/// <summary>
/// Interface for a serializer that can serialize and deserialize objects.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Deserializes a string value to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The string value to deserialize.</param>
    /// <returns>The deserialized object of type T.</returns>
    T Deserialize<T>(string value);

    /// <summary>
    /// Deserializes a ReadOnlyMemory<byte> value to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The ReadOnlyMemory<byte> value to deserialize.</param>
    /// <returns>The deserialized object of type T.</returns>
    T Deserialize<T>(in ReadOnlyMemory<byte> value);

    /// <summary>
    /// Serializes an object of type T to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized byte array.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Gets a value indicating whether the serializer is a binary serializer.
    /// </summary>
    bool IsBinarySerializer { get; }

    /// <summary>
    /// Gets a value indicating whether the serializer is a string serializer.
    /// </summary>
    bool IsStringSerializer { get; }
}
