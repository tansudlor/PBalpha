using UnityEngine;
using com.playbux.utilis.serialization;

namespace com.playbux.networking.mirror.core
{
    public class PlayerTeleportCommand : IPlayerCommand<Vector2>
    {
        public bool IsPreventOther => true;
        public ushort Id => 3;
        public ushort TickCount => tickCount;

        public int DataSize { get; }

        public Vector2 FinalPosition => finalPosition;

        public byte[] Data { get; }

        private const ushort TICK_COUNT = 15;

        private ushort tickCount = TICK_COUNT;
        private readonly Vector2 playerPosition;
        private readonly Vector2 finalPosition;

        public PlayerTeleportCommand(Vector2 playerPosition, Vector2 teleportPosition)
        {
            finalPosition = teleportPosition;
            this.playerPosition = playerPosition;
            DataSize = teleportPosition.GetDataSize();
            Data = teleportPosition.ToBytes(DataSize);
        }

        public Vector2 Perform()
        {
            tickCount--;
            return tickCount <= 0 ? FinalPosition : playerPosition;
        }

        public Vector2 Undo()
        {
            return playerPosition;
        }

        public IPlayerCommand<Vector2> Clone()
        {
            return new PlayerTeleportCommand(playerPosition, FinalPosition);
        }
    }
}