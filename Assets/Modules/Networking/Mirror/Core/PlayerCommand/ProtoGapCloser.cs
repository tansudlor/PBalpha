using com.playbux.utilis.serialization;
using Mirror;
using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    public class ProtoGapCloserCommand : IPlayerCommand<Vector2>
    {
        public bool IsPreventOther => true;
        public ushort Id => 1;
        public ushort TickCount => performCount;
        public int DataSize { get; }
        public Vector2 FinalPosition => FinalPosition;
        public byte[] Data { get; }

        private const int STEP_DIVIDER = 7;

        private readonly Vector2 step;
        private readonly Transform target;

        private ushort performCount;
        private Vector2 position;
        private NetworkIdentity targetIdentity;

        public ProtoGapCloserCommand(Vector2 position, NetworkIdentity target, float targetRadius = 4f)
        {
            DataSize = target.netId.GetDataSize();
            Data = target.netId.ToBytes(DataSize);
            targetIdentity = target;
            this.target = target.transform;
            this.position = position;
            performCount = STEP_DIVIDER;
            var targetPosition = (Vector2)this.target.position;
            var direction = (targetPosition - this.position).normalized;
            var finalPosition = targetPosition - direction * targetRadius;
            float distance = (finalPosition - position).magnitude;
            step = (finalPosition - position).normalized * (distance / STEP_DIVIDER);
        }

        public Vector2 Perform()
        {
            position += step;
            performCount--;
            return position;
        }

        public Vector2 Undo()
        {
            position -= step * STEP_DIVIDER;
            performCount = STEP_DIVIDER;
            return position;
        }

        public IPlayerCommand<Vector2> Clone()
        {
            return new ProtoGapCloserCommand(position, targetIdentity);
        }
    }

}