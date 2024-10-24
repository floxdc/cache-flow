using System;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FloxDc.CacheFlow.Serialization;

public class TextJsonSerializer : ISerializer
{
    public TextJsonSerializer(IOptions<JsonSerializerOptions>? options)
        => _options = options?.Value ?? new JsonSerializerOptions();


    public T Deserialize<T>(string value) 
        => JsonSerializer.Deserialize<T>(value, _options)!;


    public T Deserialize<T>(in ReadOnlyMemory<byte> value) 
        => JsonSerializer.Deserialize<T>(value.Span, _options)!;


    public byte[] Serialize<T>(T value) 
        => JsonSerializer.SerializeToUtf8Bytes(value, _options);


    public bool IsBinarySerializer => IsBinary;
    public bool IsStringSerializer => !IsBinary;


    private const bool IsBinary = true;

    
    private readonly JsonSerializerOptions _options;
}