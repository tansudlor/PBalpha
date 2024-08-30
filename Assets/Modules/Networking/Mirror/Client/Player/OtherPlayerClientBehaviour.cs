using Mirror;
using UnityEngine;
using com.playbux.bux;
using com.playbux.sorting;
using UnityEngine.Rendering;
using System.Collections.Generic;
using com.playbux.networking.networkavatar;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;

namespace com.playbux.networking.mirror.client
{
    public class OtherPlayerClientBehaviour : BasePlayerClientBehaviour
    {
        private double Offset => NetworkServer.sendInterval * SEND_INTERVAL_MULTIPLIER;
        private SnapshotInterpolationSettings SnapshotSettings => NetworkClient.snapshotSettings;

        private const float SEND_INTERVAL_MULTIPLIER = 1;

        private readonly IAnimator animator;
        private readonly Transform transform;
        private readonly InternalUpdateWorker updateWorker;
        private readonly SortedList<double, PositionStateSnapshot> updateSnapshots;
        private readonly INetworkMessageReceiver<TeleportationValidMessage> teleportationReceiver;
        private readonly INetworkMessageReceiver<OtherPlayerUpdatePositionMessage> updateReceiver;

        private bool isTeleporting;
        private Vector2 direction;
        private Vector2 targetPosition;

        public OtherPlayerClientBehaviour(
            NetworkAvatarBoard board,
            SortingGroup sortingGroup,
            NetworkIdentity networkIdentity,
            InternalUpdateWorker updateWorker,
            LayerSorterController layerSorterController,
            INetworkMessageReceiver<TeleportationValidMessage> teleportationReceiver,
            INetworkMessageReceiver<OtherPlayerUpdatePositionMessage> updateReceiver
            )
            : base(board, sortingGroup, networkIdentity, layerSorterController)
        {
            this.updateWorker = updateWorker;
            this.updateReceiver = updateReceiver;
            this.teleportationReceiver = teleportationReceiver;

            transform = networkIdentity.transform;

            updateSnapshots = new SortedList<double, PositionStateSnapshot>(SnapshotSettings.bufferLimit);
        }

        public override void Initialize()
        {
            updateWorker.OnTick += Tick;
            updateWorker.OnUpdateLoop += Update;
            updateReceiver.OnEventCalled += OnPositionStateReceived;
            teleportationReceiver.OnEventCalled += OnTeleportMessageReceived;
            updateWorker.Initialize();
            HandleSortingOrder();
        }

        public override void Dispose()
        {
            updateWorker.OnTick -= Tick;
            updateWorker.OnUpdateLoop -= Update;
            updateReceiver.OnEventCalled -= OnPositionStateReceived;
            teleportationReceiver.OnEventCalled -= OnTeleportMessageReceived;
            updateWorker.Dispose();
        }

        private void Update()
        {
            if (isTeleporting)
                return;

            if (updateSnapshots.Count <= 0)
                return;

            transform.position = Vector2.Distance(transform.position, targetPosition) < 10 ? Vector3.Lerp(transform.position, targetPosition, 0.1f) : targetPosition;
        }

        private void Tick()
        {
            if (isTeleporting)
            {
                HandleWalkAnimation(1, Vector2.zero);
                return;
            }

            if (updateSnapshots.Count <= 0)
            {
                HandleWalkAnimation(1, Vector2.zero);
                return;
            }

            SnapshotInterpolation.StepInterpolation(
                updateSnapshots,
                NetworkTime.time,
                out PositionStateSnapshot from,
                out PositionStateSnapshot to,
                out double t);

            // interpolate & apply
            PositionStateSnapshot computed = updateSnapshots.Count > 1 ? PositionStateSnapshot.Interpolate(from, to, t) : from;
            targetPosition = computed.Position;
            direction = updateSnapshots.Count == 1 ? Vector2.zero : from.Input;
            HandleSortingOrder();
            HandleWalkAnimation(1, direction);
        }

        private void OnTeleportMessageReceived(TeleportationValidMessage validMessage)
        {
            if (validMessage.NetId != NetworkIdentity.netId)
                return;

            isTeleporting = true;
            updateSnapshots.Clear();
            targetPosition = validMessage.TargetPosition;
            HandleSortingOrder();
            isTeleporting = false;
        }

        private void OnPositionStateReceived(OtherPlayerUpdatePositionMessage message)
        {
            if (NetworkIdentity.netId != message.NetId)
                return;

            if (isTeleporting)
                return;

            if (updateSnapshots.Count >= SnapshotSettings.bufferLimit)
                updateSnapshots.Clear();

            bool bufferIsLargerThanZero = updateSnapshots.Count > 0;
            double timeIntervalCheck = SnapshotSettings.bufferTimeMultiplier * Offset;
            double lastRecordTime = bufferIsLargerThanZero ? updateSnapshots.Keys[^1] + timeIntervalCheck : 0;

            if (bufferIsLargerThanZero && lastRecordTime < message.Timestamps[0])
                updateSnapshots.Clear();

            for (int i = 0; i < message.Position.Length; i++)
            {
                var snapshot = new PositionStateSnapshot(message.Timestamps[i] - Offset, NetworkTime.localTime, message.Inputs[i], message.Position[i]);
                SnapshotInterpolation.InsertIfNotExists(updateSnapshots, SnapshotSettings.bufferLimit, snapshot);
            }

        }
    }
}