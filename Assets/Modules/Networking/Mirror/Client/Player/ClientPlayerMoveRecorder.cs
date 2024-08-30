using System;
using Mirror;
using UnityEngine;
using com.playbux.input;
using com.playbux.motor;
using com.playbux.utilis;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;

namespace com.playbux.networking.mirror.client
{
    public class ClientPlayerMoveRecorder
    {
        public event Action OnRecord;
        public SortedList<double, InputSnapshot> InputSnapshots => inputSnapshots;
        public SortedList<double, PositionStateSnapshot> StateSnapshots => stateSnapshots;
        private SnapshotInterpolationSettings SnapshotSettings => NetworkClient.snapshotSettings;

        private readonly float moveSpeed;
        private readonly IAFKWorker afkWorker;
        private readonly NetworkIdentity networkIdentity;
        private readonly InternalUpdateWorker updateWorker;
        private readonly IInputController<Vector2> inputController;

        private bool isEnable;
        private SortedList<double, InputSnapshot> inputSnapshots;
        private SortedList<double, PositionStateSnapshot> stateSnapshots;

        public ClientPlayerMoveRecorder(
            IMotor motor,
            IAFKWorker afkWorker,
            NetworkIdentity networkIdentity,
            InternalUpdateWorker updateWorker,
            IInputController<Vector2> inputController
            )
        {
            moveSpeed = motor.MoveSpeed;
            this.afkWorker = afkWorker;
            this.updateWorker = updateWorker;
            this.inputController = inputController;
            this.networkIdentity = networkIdentity;

            inputSnapshots = new SortedList<double, InputSnapshot>(SnapshotSettings.bufferLimit);
            stateSnapshots = new SortedList<double, PositionStateSnapshot>(SnapshotSettings.bufferLimit);
        }

        public void Initialize()
        {
            updateWorker.OnTick += Tick;
            afkWorker.Initialize();
            updateWorker.Initialize();
        }

        public void Dispose()
        {
            updateWorker.OnTick -= Tick;
            afkWorker.Dispose();
            updateWorker.Dispose();
        }

        private void Tick()
        {
            if (afkWorker.IsAFK)
                return;

            RecordLocalState();
        }

        public bool CanBeSent()
        {
            return isEnable && inputSnapshots.Count > 0;
        }

        public void SetActive(bool enabled)
        {
            isEnable = enabled;
        }

        private void RecordLocalState()
        {
            if (inputSnapshots.Count > 0 && inputSnapshots.Values[^1].Input == Vector2.zero && inputController.Value == Vector2.zero)
                return;

            var inputSnapshot = new InputSnapshot(NetworkTime.time, NetworkTime.localTime, inputController.Value);
            var stateSnapshot = new PositionStateSnapshot(NetworkTime.time, NetworkTime.localTime, inputController.Value, networkIdentity.transform.position);
            SnapshotInterpolation.InsertIfNotExists(inputSnapshots, SnapshotSettings.bufferLimit, inputSnapshot);
            SnapshotInterpolation.InsertIfNotExists(stateSnapshots, SnapshotSettings.bufferLimit, stateSnapshot);

            OnRecord?.Invoke();
        }

        public void OnPositionStateReceived(PlayerUpdatePositionMessage message)
        {
//             if (networkIdentity.netId != message.NetId)
//                 return;
//
//             if (StateSnapshots.Count >= SnapshotSettings.bufferLimit)
//                 StateSnapshots.Clear();
//
//             if (StateSnapshots.Count + message.Position.Length > SnapshotSettings.bufferLimit)
//                 StateSnapshots.Clear();
//
//             bool bufferIsLargerThanZero = StateSnapshots.Count > 0;
//             float distance = 0f;
//             double lastRecordTime = bufferIsLargerThanZero ? StateSnapshots.Keys[^1] : 0;
//
//             // If the newest state in the buffer is ahead latest server message than 6 ticks, then predict the last local snapshot state.
//             // If distance between that predicted position and server position is more than 1f then apply position immediately.
//             // Due to late arrival like this means client prediction is likely going to be wrong.
//             if (bufferIsLargerThanZero && lastRecordTime < message.Timestamps[^1])
//             {
//                 Vector3 lastVelocity = (message.Inputs[^1].ToDirection() * moveSpeed) * 4;
//                 Vector3 predicted = message.Position[^1] + lastVelocity;
//                 distance = Vector3.Distance(networkIdentity.transform.position, predicted);
//                 float threshold = 5f + moveSpeed * (1 + ((float)NetworkTime.rtt * 0.5f));
//
//                 if (distance > threshold)
//                 {
//                     networkIdentity.transform.position = message.Position[^1];
// #if DEVELOPMENT
//                     Debug.Log($"Reconciled due to late arrival: distance is {distance}");
// #endif
//                 }
//             }
//
//             for (int i = 0; i < message.Position.Length; i++)
//             {
//                 var snapshot = new PositionStateSnapshot(message.Timestamps[i], NetworkTime.localTime, message.Inputs[i], message.Position[i]);
//                 SnapshotInterpolation.InsertIfNotExists(StateSnapshots, SnapshotSettings.bufferLimit, snapshot);
//             }
        }
    }
}