using System;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.events;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;

namespace com.playbux.networking.mirror.server
{
    public class ServerPlayerMoveRecorder
    {
        public event Action OnReconciled;
        public event Action<double> OnLateMessage;
        public event Action<Vector2> OnInputProcessed;

        public Vector2 CurrentInput;

        public SortedList<double, InputSnapshot> InputBuffer => inputBuffer;
        public SortedList<double, PositionStateSnapshot> PositionBuffer => positionBuffer;

        private int BufferSize => NetworkClient.snapshotSettings.bufferLimit;
        private double Offset => NetworkServer.sendInterval * SEND_INTERVAL_MULTIPLIER;
        private double BufferMultiplier => NetworkClient.snapshotSettings.bufferTimeMultiplier;

        private const float SEND_INTERVAL_MULTIPLIER = 2f;

        private readonly Transform transform;
        private readonly NetworkIdentity networkIdentity;
        private readonly InternalUpdateWorker internalUpdateWorker;

        private bool isTeleporting;
        private bool isReconciling;
        private bool isAnticipateDisconnection;
        private int disconnectionCounter;
        private int recordIntervalCounter;
        private double lastRecordInterval;
        private double lastDisconnectInterval;

        private SortedList<double, InputSnapshot> inputBuffer;
        private SortedList<double, PositionStateSnapshot> positionBuffer;

        public ServerPlayerMoveRecorder(
            NetworkIdentity networkIdentity,
            InternalUpdateWorker internalUpdateWorker
            )
        {
            this.networkIdentity = networkIdentity;
            this.internalUpdateWorker = internalUpdateWorker;

            transform = this.networkIdentity.transform;
            inputBuffer = new SortedList<double, InputSnapshot>(BufferSize);
            positionBuffer = new SortedList<double, PositionStateSnapshot>(BufferSize);
        }

        public void Initialize()
        {
            internalUpdateWorker.OnTick += OnTick;
            internalUpdateWorker.Initialize();
        }

        public void Dispose()
        {
            internalUpdateWorker.OnTick -= OnTick;
            internalUpdateWorker.Dispose();
        }

        private void OnTick()
        {
            ProcessInput();
        }

        private void ProcessInput()
        {
            if (isReconciling)
                return;

            if (inputBuffer.Count == 0)
                return;

            InputSnapshot from = inputBuffer.Values[0];

            // Only apply when 'from' timestamp is more than current time
            if (from.remoteTime >= NetworkTime.time)
                return;

            CurrentInput = from.Input;
            OnInputProcessed?.Invoke(CurrentInput);

            if (inputBuffer.Count <= 0)
                return;

            inputBuffer.RemoveAt(0);
            RecordState();
        }

        private void RecordState()
        {
            var snapshot = new PositionStateSnapshot(NetworkTime.localTime, NetworkTime.localTime, CurrentInput, transform.position);
            switch (positionBuffer.Count)
            {
                case 0:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    return;
                case > 0 when positionBuffer.Values[^1].Input == Vector2.zero && CurrentInput == Vector2.zero:
                    return;
                default:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    break;
            }
        }

        private void AddInputSnapshot(PlayerMoveInputMessage message)
        {
            for (int i = 0; i < message.Inputs.Length; i++)
            {
                var snapshot = new InputSnapshot(message.Timestamps[i] + Offset, NetworkTime.time, message.Inputs[i]);
                SnapshotInterpolation.InsertIfNotExists(inputBuffer, BufferSize, snapshot);
            }
        }

        public void OnInputMessageReceived(NetworkConnectionToClient connection, PlayerMoveInputMessage message, int channel)
        {
            if (connection.identity.netId != networkIdentity.netId)
                return;

            // Check if player message is unacceptably delay then we can start anticipating disconnection
            OnLateMessage?.Invoke(message.Timestamps[^1]);

            //If the input buffer is equal or more than buffer size then apply all input in the buffer immediately, clear the buffer then add the message received to the buffer
            if (inputBuffer.Count >= BufferSize)
            {
                OnReconciled?.Invoke();
                AddInputSnapshot(message);
                return;
            }

            //BufferMultiplier is 3 and Offset is about 1s (SEND_INTERVAL_MULTIPLIER = 3) * (NetworkServer.sendInterval = 1 / 3 = 0.33s)
            // so in total of about 6 ticks
            double timeIntervalCheck = BufferMultiplier * Offset;

            //If the newest input in the buffer is older than 6 ticks, then apply all input in the buffer immediately, clear the buffer then add the message received to the buffer
            if (inputBuffer.Count > 0 && inputBuffer.Values[^1].remoteTime + timeIntervalCheck < message.Timestamps[^1])
            {
                OnReconciled?.Invoke();
#if DEVELOPMENT
                Debug.Log("Reconciled due to delayed arrival");
#endif
            }

            AddInputSnapshot(message);
        }
    }
}