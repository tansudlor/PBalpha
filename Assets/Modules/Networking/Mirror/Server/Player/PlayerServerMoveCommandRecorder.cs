using System;
using Mirror;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.utilis.serialization;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;
using com.playbux.networking.server.stepcount;
using UnityEngine.UIElements;

namespace com.playbux.networking.mirror.server
{
    public class PlayerServerMoveCommandRecorder : ITickable
    {
        public event Func<float, Vector2, Vector2, Vector2> OnTeleportTrigger;
        public event Func<float, Vector2, Vector2, Vector2> OnColliderTrigger;

        private const ushort MAX_FRAME = 200;
        private const ushort VALIDATE_FRAME = 6;
        private const float MOVE_SPEED = 0.24f;
        private const float SEND_INTERVAL_MULTIPLIER = 1;
        private double Offset => NetworkServer.sendInterval * SEND_INTERVAL_MULTIPLIER;
        private int BufferSize => NetworkClient.snapshotSettings.bufferLimit;

        private IIdentitySystem identitySystem;
        private readonly StepCounter stepCounter;
        private readonly NetworkIdentity networkIdentity;
        private readonly IServerNetworkMessageReceiver<MoveCommandMessage> moveCommandReceiver;
        private readonly IServerMessageSender<OtherPlayerUpdatePositionMessage> otherPlayerUpdatePositionSender;

        private bool isInterrupting;
        private ushort frameCount;
        private ushort lastCommandFrame;
        private Vector3 targetPosition;
        private Dictionary<ushort, Vector2> resyncBuffer;
        private Dictionary<ushort, ServerCommandData> commandQueue;
        private Dictionary<ushort, IPlayerCommand<Vector2>> processedQueue;
        private SortedList<double, PositionStateSnapshot> positionBuffer;

        private IServerNetworkMessageReceiver<ResyncMessage> resyncMessage;

        public PlayerServerMoveCommandRecorder(
            StepCounter stepCounter,
            IIdentitySystem identitySystem,
            NetworkIdentity networkIdentity,
            IServerNetworkMessageReceiver<MoveCommandMessage> moveCommandReceiver,
            IServerMessageSender<OtherPlayerUpdatePositionMessage> otherPlayerUpdatePositionSender,
            IServerNetworkMessageReceiver<ResyncMessage> resyncMessage)
        {
            this.stepCounter = stepCounter;
            this.identitySystem = identitySystem;
            this.networkIdentity = networkIdentity;
            this.moveCommandReceiver = moveCommandReceiver;
            this.otherPlayerUpdatePositionSender = otherPlayerUpdatePositionSender;
            this.resyncMessage = resyncMessage;

            resyncBuffer = new Dictionary<ushort, Vector2>();
            commandQueue = new Dictionary<ushort, ServerCommandData>();
            processedQueue = new Dictionary<ushort, IPlayerCommand<Vector2>>();
            positionBuffer = new SortedList<double, PositionStateSnapshot>(BufferSize);
        }

        public void Initialize()
        {
            resyncMessage.OnEventCalled += OnResync;
            moveCommandReceiver.OnEventCalled += OnMoveCommandReceived;
            otherPlayerUpdatePositionSender.MessageToObserver += SendStatesMessage;
            otherPlayerUpdatePositionSender.SendCondition += SendStateMessageCondition;

        }

        public void Dispose()
        {
            resyncMessage.OnEventCalled -= OnResync;
            moveCommandReceiver.OnEventCalled -= OnMoveCommandReceived;
            otherPlayerUpdatePositionSender.MessageToObserver -= SendStatesMessage;
            otherPlayerUpdatePositionSender.SendCondition -= SendStateMessageCondition;
        }

        public void Tick()
        {
            if (isInterrupting)
                return;

            IPlayerCommand<Vector2> command = null;
            targetPosition = networkIdentity.transform.position;

            if (commandQueue.ContainsKey(frameCount))
            {
                if (!processedQueue.ContainsKey(frameCount))
                {
                    var commandData = commandQueue[frameCount];

                    if (commandData.id == 0)
                    {
                        var data = commandData.data.FromBytes<InputValue>(commandData.dataSize);
                        processedQueue[frameCount] = new PlayerMoveCommand(MOVE_SPEED, networkIdentity.transform.position, data.verticalInput, data.horizontalInput);
                        stepCounter.Count(identitySystem[networkIdentity.netId].UID);
                    }
                    else
                    {
                        var data = commandData.data.FromBytes<uint>(commandData.dataSize);
                        if (NetworkServer.spawned.TryGetValue(data, out var targetIdentity))
                            processedQueue[frameCount] = new ProtoGapCloserCommand(networkIdentity.transform.position, targetIdentity);
                    }
                }

                lastCommandFrame = frameCount;
                commandQueue.Remove(frameCount);
                command = processedQueue[frameCount];
            }
            else
            {
                if (processedQueue.ContainsKey(lastCommandFrame))
                    command = processedQueue[lastCommandFrame];

                if (processedQueue.ContainsKey(frameCount))
                    command = processedQueue[frameCount];

                if (command == null)
                {
                    frameCount++;
                    frameCount %= MAX_FRAME;
                    return;
                }
            }

            if (isInterrupting)
                return;

            targetPosition = command.Perform();
            Vector2 direction = targetPosition - networkIdentity.transform.position;
            Vector2 directionForCollider = direction;
            direction.x *= -1;
            direction.Normalize();
            directionForCollider.Normalize();

            if (OnTeleportTrigger != null && command.Id == 0)
                targetPosition = OnTeleportTrigger?.Invoke(MOVE_SPEED, directionForCollider, networkIdentity.transform.position) ?? targetPosition;

            if (OnColliderTrigger != null && command.Id == 0)
                targetPosition = OnColliderTrigger?.Invoke(MOVE_SPEED, directionForCollider, networkIdentity.transform.position) ?? targetPosition;

            RecordForOther(direction, targetPosition);
            RecordValidateBuffer(frameCount, targetPosition);
            networkIdentity.transform.position = targetPosition;
            SendMoveValidateMessage(frameCount, command.Id, targetPosition);
            frameCount++;
            frameCount %= MAX_FRAME;

            if (command.TickCount > 0)
                return;

            processedQueue.Remove(lastCommandFrame);
        }

        public void InterruptCommand(Vector2 position)
        {
            isInterrupting = true;
            frameCount = lastCommandFrame = 0;
            commandQueue.Clear();
            processedQueue.Clear();
            positionBuffer.Clear();
            networkIdentity.transform.position = position;
            DelayUnlockInterrupt().Forget();
        }

        private async UniTaskVoid DelayUnlockInterrupt()
        {
            await UniTask.WaitForSeconds((float)Offset);
            isInterrupting = false;
        }

        private void SendMoveValidateMessage(ushort frame, uint id, Vector2 position)
        {
            if (frame % VALIDATE_FRAME != 0)
                return;

            networkIdentity.connectionToClient.Send(new MoveCommandValidationMessage(networkIdentity.netId, lastCommandFrame, id, position));
        }

        private void RecordValidateBuffer(ushort frame, Vector2 position)
        {
            resyncBuffer[frame] = position;
        }

        private void RecordForOther(Vector2 direction, Vector2 position)
        {
            var snapshot = new PositionStateSnapshot(NetworkTime.localTime, NetworkTime.localTime, direction, position);
            switch (positionBuffer.Count)
            {
                case 0:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    return;
                case > 0 when positionBuffer.Values[^1].Input == Vector2.zero && direction == Vector2.zero:
                    return;
                default:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    break;
            }
        }

        private bool SendStateMessageCondition()
        {
            return positionBuffer.Count > 0;
        }

        private ServerMessageToObserver<OtherPlayerUpdatePositionMessage> SendStatesMessage()
        {
            double[] timestamps = positionBuffer.Keys.ToArray();
            var inputs = positionBuffer.Values.Select(value => value.Input).ToArray();
            var positions = positionBuffer.Values.Select(value => value.Position).ToArray();
            positionBuffer.Clear();
            var toOtherMessage = new OtherPlayerUpdatePositionMessage(networkIdentity.netId, timestamps, inputs, positions);
            return new ServerMessageToObserver<OtherPlayerUpdatePositionMessage>(true, networkIdentity, toOtherMessage);
        }

        private void OnMoveCommandReceived(NetworkConnectionToClient connectionToClient, MoveCommandMessage message, int channel)
        {
            if (message.NetId != networkIdentity.netId)
                return;

            if (isInterrupting)
                return;

            var commandData = new ServerCommandData
            {
                id = message.CommandId,
                data = message.CommandDatas,
                dataSize = message.CommandDataSize
            };
            commandQueue[message.Frame] = commandData;
        }

        private void OnResync(NetworkConnectionToClient connectionToClient, ResyncMessage message, int channel)
        {
            if (message.NetId != networkIdentity.netId)
                return;

            if (!resyncBuffer.ContainsKey(message.Frame))
            {
                connectionToClient.Disconnect(); // You cheating scums get tf out of my game
                return;
            }

            InterruptCommand(resyncBuffer[message.Frame]);
            resyncBuffer.Clear();
        }
    }
}