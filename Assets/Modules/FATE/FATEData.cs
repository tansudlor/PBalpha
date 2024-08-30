#nullable enable
using System;
using UnityEngine;

namespace com.playbux.FATE
{
    [Serializable]
    public struct FATEData : IEquatable<FATEData>
    {
        public uint id;
        public string name;
        public string desc;

        public bool Equals(FATEData other)
        {
            return id == other.id && name == other.name;
        }
        public override bool Equals(object? obj)
        {
            return obj is FATEData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id, name);
        }

        public FATEData Clone()
        {
            return new FATEData
            {
                id = id,
                name = name,
                desc = desc
            };
        }
    }

    [Serializable]
    public struct FATEPosition : IEquatable<FATEPosition>
    {
        public bool random;
        public Vector2[] values;

        public bool Equals(FATEPosition other)
        {
            return random == other.random && values.Equals(other.values);
        }
        public override bool Equals(object? obj)
        {
            return obj is FATEPosition other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(random, values);
        }
    }
}