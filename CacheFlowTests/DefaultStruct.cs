using System;
using System.Text.Json.Serialization;

namespace CacheFlowTests
{
    [Serializable]
    public struct DefaultStruct
    {
        [JsonConstructor]
        public DefaultStruct(int id)
        {
            Id = id;
        }


        public override bool Equals(object other)
        {
            if (other is DefaultStruct defaultStruct)
                return Equals(defaultStruct);

            return false;
        }


        private bool Equals(DefaultStruct other)
        {
            return Id == other.Id;
        }


        public override int GetHashCode()
        {
            return Id;
        }


        public int Id { get; }
    }
}
