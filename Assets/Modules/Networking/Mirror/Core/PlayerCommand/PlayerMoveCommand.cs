using UnityEngine;
using com.playbux.utilis.serialization;

namespace com.playbux.networking.mirror.core
{
    public static class PlayerMoveCommandHelper
    {
        private const int MOVE_COMMAND_TICK_COUNT = 1;
        private const int PROTO_GAP_CLOSER_COMMAND_TICK_COUNT = 7;
        private const int TELEPORT_COMMAND_TICK_COUNT = 15;
        
        public static ushort GetTickCount(this ushort commandId)
        {
            return commandId switch
            {
                0 => MOVE_COMMAND_TICK_COUNT,
                1 => PROTO_GAP_CLOSER_COMMAND_TICK_COUNT,
                2 => TELEPORT_COMMAND_TICK_COUNT,
                _ => 0
            };
        }
    }
    
    public class PlayerMoveCommand : IPlayerCommand<Vector2>
    {
        public bool IsPreventOther => false;
        public ushort Id => 0;
        public ushort TickCount => tickCount;
        public int DataSize { get; }
        public Vector2 FinalPosition { get; }
        public byte[] Data { get; }

        private readonly float moveSpeed;
        private readonly InputValue inputValue;

        private Vector2 currentPosition;

        private ushort tickCount = 1;

        public PlayerMoveCommand(
            float moveSpeed,
            Vector2 currentPosition,
            VerticalInput verticalInput,
            HorizontalInput horizontalInput)
        {
            this.moveSpeed = moveSpeed;
            this.currentPosition = currentPosition;
            inputValue = new InputValue();
            inputValue.verticalInput = verticalInput;
            inputValue.horizontalInput = horizontalInput;
            DataSize = inputValue.GetDataSize();
            Data = inputValue.ToBytes(DataSize);
            var velocity = new Vector2((short)inputValue.horizontalInput, (short)inputValue.verticalInput);
            velocity.Normalize();
            FinalPosition = this.currentPosition + velocity * this.moveSpeed;
        }

        public Vector2 Perform()
        {
            var velocity = new Vector2((short)inputValue.horizontalInput, (short)inputValue.verticalInput);
            velocity.Normalize();
            currentPosition += velocity * moveSpeed;
            tickCount--;
            return currentPosition;
        }

        public Vector2 Undo()
        {
            var velocity = new Vector2((short)inputValue.horizontalInput, (short)inputValue.verticalInput);
            velocity.Normalize();
            currentPosition -= velocity * moveSpeed;
            return currentPosition;
        }

        public IPlayerCommand<Vector2> Clone()
        {
            return new PlayerMoveCommand(moveSpeed, currentPosition, inputValue.verticalInput, inputValue.horizontalInput);
        }
    }
}