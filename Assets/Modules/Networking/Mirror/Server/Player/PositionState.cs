using System;
using UnityEngine;

namespace com.playbux.networking.mirror.server
{
    public struct PositionState : IEquatable<PositionState>
    {
        public Vector2 Input => input;

        public Vector3 Position => position;

        private readonly Vector2 input;
        private readonly Vector3 position;
        
        public PositionState(Vector2 input, Vector3 position)
        {
            this.input = input;
            this.position = position;
        }

        public bool Equals(PositionState other)
        {
            return input.Equals(other.input) && position.Equals(other.position);
        }

        public override bool Equals(object obj)
        {
            return obj is PositionState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(input, position);
        }
    }
}