using com.playbux.utilis;
using Mirror;
using UnityEngine;

namespace com.playbux.networking.mirror.snapshot
{
    public struct InputSnapshot : Snapshot
    { 
        public double remoteTime { get; set; }
        public double localTime { get; set; }
        public Vector2 Input => input;
        
        private readonly Vector2 input;

        public InputSnapshot(double remoteTime, double localTime, Vector2 input)
        {
            this.localTime = localTime;
            this.remoteTime = remoteTime;
            this.input = input;
        }

        public InputSnapshot Clone()
        {
            return new InputSnapshot(remoteTime, localTime, input);
        }
    }

    public struct PositionStateSnapshot : Snapshot
    {
        public double remoteTime { get; set; }
        public double localTime { get; set; }
        public Vector2 Input => input;
        public Vector3 Position => position;

        private readonly Vector2 input;
        private readonly Vector3 position;
        
        public PositionStateSnapshot(double remoteTime, double localTime, Vector2 input, Vector3 position)
        {
            this.localTime = localTime;
            this.remoteTime = remoteTime;
            this.input = input;
            this.position = position;
        }
        
        public static PositionStateSnapshot Interpolate(PositionStateSnapshot from, PositionStateSnapshot to, double t)
        {
            return new PositionStateSnapshot(0, 0, from.input, Vector3.LerpUnclamped(from.position, to.position, (float)t));
        }
    }
}