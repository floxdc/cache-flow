﻿using System;
using System.Text.Json.Serialization;

namespace CacheFlowTests;

[Serializable]
public class DefaultClass
{
    [JsonConstructor]
    public DefaultClass(int id)
    {
        Id = id;
    }


    public override bool Equals(object other)
    {
        if (other is DefaultClass defaultClass)
            return Equals(defaultClass);

        return false;
    }


    private bool Equals(DefaultClass other)
    {
        return Id == other.Id;
    }


    public override int GetHashCode()
    {
        return Id;
    }


    public int Id { get; set; }
}