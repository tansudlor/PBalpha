using System;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.input;
using UnityEngine.Assertions;
using System.Collections.Generic;
using com.playbux.utilis.serialization;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.networking.client.targetable;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace com.playbux.networking.mirror.client
{
    public class PlayerClientMoveCommandRecorder : ITickable
    {
        public event Action OnMove;

        public event Func<float, Vector2, Vector2, Vector2> OnTeleportTrigger;
        public event Func<float, Vector2, Vector2, Vector2> OnColliderTrigger;

        private const ushort MAX_FRAME = 200;
        private const float MOVE_SPEED = 0.24f;
        private const float RECONCILE_DISTANCE = 3f;

        private readonly NetworkIdentity networkIdentity;
        private readonly InternalUpdateWorker updateWorker;
        private readonly IInputController<Vector2> inputController;
        private readonly ClientTargetableController targetableController;
        private readonly INetworkMessageReceiver<MoveCommandValidationMessage> moveCommandValidator;

        private bool isInterrupting;
        private bool isPollingEnabled;
        private bool isPreventByCommands;
        private bool gapCloserDown;
        private ushort frameCount;
        private ushort pollingFrame;
        private ushort lastCommandFrame;
        private IPlayerCommand<Vector2> interruptCommand = null;
        private Dictionary<ushort, Vector2> processedPosition;
        private CancellationTokenSource cancellationTokenSource;
        private Dictionary<ushort, IPlayerCommand<Vector2>> commandQueue;
        private Dictionary<ushort, IPlayerCommand<Vector2>> processedQueue;

        
        public PlayerClientMoveCommandRecorder(
            NetworkIdentity networkIdentity,
            InternalUpdateWorker updateWorker,
            IInputController<Vector2> inputController,
            ClientTargetableController targetableController,
            INetworkMessageReceiver<MoveCommandValidationMessage> moveCommandValidator)
        {
            this.updateWorker = updateWorker;
            this.networkIdentity = networkIdentity;
            this.inputController = inputController;
            this.targetableController = targetableController;
            this.moveCommandValidator = moveCommandValidator;

            processedPosition = new Dictionary<ushort, Vector2>();
            cancellationTokenSource = new CancellationTokenSource();
            commandQueue = new Dictionary<ushort, IPlayerCommand<Vector2>>();
            processedQueue = new Dictionary<ushort, IPlayerCommand<Vector2>>();
        }

        public void Initialize()
        {
            updateWorker.OnTick += Polling;
            moveCommandValidator.OnEventCalled += OnMoveCommandValidateMessage;
            updateWorker.Initialize();
            EnablePolling(true);
            //resyncMessage.OnEventCalled += OnResync;
        }
        public void Dispose()
        {
            updateWorker.OnTick -= Polling;
            moveCommandValidator.OnEventCalled -= OnMoveCommandValidateMessage;
            updateWorker.Dispose();
        }

        public void Tick()
        {
            if (isInterrupting)
                return;

            if (commandQueue.Count <= 0)
                return;

            ushort processedFrame = commandQueue.ContainsKey(frameCount) ? frameCount : lastCommandFrame;
            Assert.IsTrue(commandQueue.ContainsKey(processedFrame));
            processedQueue[processedFrame] = commandQueue[processedFrame];

            if (isInterrupting)
                return;

            var targetPosition = commandQueue[processedFrame].Perform();
            Vector2 direction = targetPosition - (Vector2)networkIdentity.transform.position;
            direction.Normalize();

            if (OnTeleportTrigger != null && commandQueue[processedFrame].Id == 0)
                targetPosition = OnTeleportTrigger?.Invoke(MOVE_SPEED, direction, networkIdentity.transform.position) ?? targetPosition;

            if (OnColliderTrigger != null && commandQueue[processedFrame].Id == 0)
                targetPosition = OnColliderTrigger?.Invoke(MOVE_SPEED, direction, networkIdentity.transform.position) ?? targetPosition;

            isPreventByCommands = commandQueue[processedFrame].IsPreventOther;
            networkIdentity.transform.position = Vector3.Lerp(networkIdentity.transform.position, targetPosition, 1f);
            OnMove?.Invoke();
            processedPosition[frameCount] = targetPosition;
            frameCount++;
            frameCount %= MAX_FRAME;

#if UNITY_EDITOR || DEBUG
            Debug.DrawLine(networkIdentity.transform.position, commandQueue[processedFrame].FinalPosition, Color.green, 1f);
#endif

            if (commandQueue[processedFrame].TickCount > 0)
                return;

            isPreventByCommands = false;
            commandQueue.Remove(processedFrame);
            lastCommandFrame = frameCount;
        }

        public void EnablePolling(bool isEnabled)
        {
            isPollingEnabled = isEnabled;
        }

        private void Polling()
        {
            if (isInterrupting)
                return;

            if (!isPollingEnabled)
                return;

            if (isPreventByCommands)
                return;

            if (Input.GetKeyDown(KeyCode.O))
                gapCloserDown = true;

            if (Input.GetKeyUp(KeyCode.O) && gapCloserDown)
            {
                gapCloserDown = false;
                var gridIndexes = NetworkClient.aoi.GetPosition(networkIdentity.transform.position);
                var target = targetableController.Target(networkIdentity.netId, null, gridIndexes);

                if (target == null)
                    return;

                var gapCloserCommand = new ProtoGapCloserCommand(networkIdentity.transform.position, target);
                NetworkClient.Send(new MoveCommandMessage(networkIdentity.netId, pollingFrame, gapCloserCommand.Id, gapCloserCommand.Data, gapCloserCommand.DataSize));
                commandQueue[pollingFrame] = gapCloserCommand;
                pollingFrame += gapCloserCommand.TickCount;
                pollingFrame %= MAX_FRAME;
                return;
            }

            var input = inputController.Value;
            if (input == Vector2.zero)
                return;

            VerticalInput verticalInput = input.y == 0 ? VerticalInput.None : (input.y > 0 ? VerticalInput.Up : VerticalInput.Down);
            HorizontalInput horizontalInput = input.x == 0 ? HorizontalInput.None : (input.x > 0 ? HorizontalInput.Left : HorizontalInput.Right);
            var moveCommand = new PlayerMoveCommand(MOVE_SPEED, networkIdentity.transform.position, verticalInput, horizontalInput);
            NetworkClient.Send(new MoveCommandMessage(networkIdentity.netId, pollingFrame, moveCommand.Id, moveCommand.Data, moveCommand.DataSize));
            commandQueue[pollingFrame] = moveCommand;
            pollingFrame += moveCommand.TickCount;
            pollingFrame %= MAX_FRAME;
        }

        private void OnMoveCommandValidateMessage(MoveCommandValidationMessage message)
        {
            if (message.NetId != networkIdentity.netId)
                return;

            bool hasProcessedCommand = processedQueue.ContainsKey(message.Frame);

            if (!hasProcessedCommand)
            {
                Reconcile(message.Frame, message.Position).Forget();
                return;
            }

            bool isProcessedCommandMatched = processedQueue[message.Frame].Id == message.CommandId;
            float distance = Vector2.Distance(processedQueue[message.Frame].FinalPosition, message.Position); 

            if (isProcessedCommandMatched && distance < RECONCILE_DISTANCE)
                return;

            bool hasSnapshotPosition = processedPosition.ContainsKey(message.Frame);
            distance = Vector2.Distance(processedPosition[message.Frame], message.Position);

            if (hasSnapshotPosition && distance < RECONCILE_DISTANCE)
                return;

            Reconcile(message.Frame, message.Position).Forget();
        }

        private async UniTaskVoid Reconcile(ushort frame, Vector2 position)
        {
            networkIdentity.transform.position = position;
            frameCount = 0;
            pollingFrame = 0;
            lastCommandFrame = 0;
            commandQueue.Clear();
            processedQueue.Clear();
            processedPosition.Clear();
            EnablePolling(false);
            NetworkClient.Send(new ResyncMessage(networkIdentity.netId, frame));
            await UniTask.Delay(1000, true, PlayerLoopTiming.FixedUpdate, cancellationTokenSource.Token);
            EnablePolling(true);
        }
    }
}