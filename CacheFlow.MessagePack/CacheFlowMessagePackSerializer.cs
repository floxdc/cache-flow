﻿using System;
using FloxDc.CacheFlow.Serialization;
using MessagePack;
using MessagePack.Resolvers;

namespace CacheFlow.MessagePack;

public class CacheFlowMessagePackSerializer : ISerializer
{
    public CacheFlowMessagePackSerializer(MessagePackSerializerOptions? options = null, params IFormatterResolver[]? resolvers)
    {
        resolvers ??= [StandardResolver.Instance];
        var resolver = CompositeResolver.Create(resolvers);
        
        options ??= MessagePackSerializerOptions.Standard;
        _options = options.WithResolver(resolver);
    }


    public T Deserialize<T>(string _) 
        => throw new NotImplementedException("The string overload is disabled for binary formatters");


    public T Deserialize<T>(in ReadOnlyMemory<byte> value) 
        => MessagePackSerializer.Deserialize<T>(value, _options);


    public byte[] Serialize<T>(T value) 
        => MessagePackSerializer.Serialize(value, _options);


    public bool IsBinarySerializer => IsBinary;
    public bool IsStringSerializer => !IsBinary;


    private const bool IsBinary = true;
    private readonly MessagePackSerializerOptions _options;
}
