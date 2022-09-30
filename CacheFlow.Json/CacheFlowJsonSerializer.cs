using System;
using System.Text;
using FloxDc.CacheFlow.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CacheFlow.Json;

public class CacheFlowJsonSerializer : ISerializer
{
    public CacheFlowJsonSerializer(IOptions<JsonSerializerSettings>? options)
    {
        _options = options?.Value ?? new JsonSerializerSettings();
    }


    public T Deserialize<T>(string value) 
        => JsonConvert.DeserializeObject<T>(value, _options)!;


    public T Deserialize<T>(in ReadOnlyMemory<byte> _) 
        => throw new NotImplementedException("The ReadOnlyMemory<byte> overload is disabled for string formatters");


    byte[] ISerializer.Serialize<T>(T value)
    {
        var serialized = JsonConvert.SerializeObject(value, _options);
        return Encoding.UTF8.GetBytes(serialized);
    }


    public bool IsBinarySerializer => IsBinary;
    public bool IsStringSerializer => !IsBinary;


    private const bool IsBinary = false;


    private readonly JsonSerializerSettings _options;
}