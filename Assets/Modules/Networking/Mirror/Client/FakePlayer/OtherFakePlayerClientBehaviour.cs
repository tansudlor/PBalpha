using Mirror;
using UnityEngine;
using com.playbux.bux;
using com.playbux.avatar;
using com.playbux.sorting;
using UnityEngine.Rendering;
using System.Collections.Generic;
using com.playbux.events;
using com.playbux.networking.mirror.core;
using Animator = UnityEngine.Animator;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;
using com.playbux.settings;

namespace com.playbux.networking.mirror.client.fakeplayer
{
    public class OtherFakePlayerClientBehaviour : BaseFakePlayerClientBehaviour
    {
        private double Offset => NetworkServer.sendInterval * SEND_INTERVAL_MULTIPLIER;
        private SnapshotInterpolationSettings SnapshotSettings => NetworkClient.snapshotSettings;

        private const float SEND_INTERVAL_MULTIPLIER = 1;

        private readonly Transform transform;
        private readonly PartSwapper partSwapper;
        private readonly FakePlayerIdentity identity;
        private readonly InternalUpdateWorker updateWorker;
        private readonly SortedList<double, PositionStateSnapshot> updateSnapshots;
        private readonly INetworkMessageReceiver<FakePlayerPartMessage> partMessageReceiver;
        private readonly INetworkMessageReceiver<FakePlayerPositionMessage> positionMessageReceiver;
        private readonly INetworkMessageReceiver<FakePlayerNameChangeMessage> nameChangeMessageReceiver;

        private bool isTeleporting;
        private Vector2 direction;
        private Vector2 targetPosition;
        private IAvatarSet avatarSet;

        public OtherFakePlayerClientBehaviour(
            IAnimator animator,
            PartSwapper partSwapper,
            Animator shadowAnimator,
            SortingGroup sortingGroup,
            FakePlayerIdentity identity,
            NetworkIdentity networkIdentity,
            InternalUpdateWorker updateWorker,
            PartDirectionWorker partDirectionWorker,
            LayerSorterController layerSorterController,
            INetworkMessageReceiver<FakePlayerPartMessage> partMessageReceiver,
            INetworkMessageReceiver<FakePlayerPositionMessage> positionMessageReceiver,
            INetworkMessageReceiver<FakePlayerNameChangeMessage> nameChangeMessageReceiver
        )
            : base(animator, shadowAnimator, sortingGroup, networkIdentity, partDirectionWorker, layerSorterController)
        {
            avatarSet = new AvatarSet();
            this.identity = identity;
            this.partSwapper = partSwapper;
            this.updateWorker = updateWorker;
            this.partMessageReceiver = partMessageReceiver;
            this.positionMessageReceiver = positionMessageReceiver;
            this.nameChangeMessageReceiver = nameChangeMessageReceiver;

            transform = networkIdentity.transform;

            updateSnapshots = new SortedList<double, PositionStateSnapshot>(SnapshotSettings.bufferLimit);
        }

        public override void Initialize()
        {
            updateWorker.OnTick += Tick;
            updateWorker.OnUpdateLoop += Update;
            partMessageReceiver.OnEventCalled += OnPartMessageReceived;
            nameChangeMessageReceiver.OnEventCalled += OnNameChangeMessageReceived;
            positionMessageReceiver.OnEventCalled += OnPositionStateMessageReceived;
            updateWorker.Initialize();
            HandleSortingOrder();
        }

        public override void Dispose()
        {
            updateWorker.OnTick -= Tick;
            updateWorker.OnUpdateLoop -= Update;
            partMessageReceiver.OnEventCalled -= OnPartMessageReceived;
            nameChangeMessageReceiver.OnEventCalled -= OnNameChangeMessageReceived;
            positionMessageReceiver.OnEventCalled -= OnPositionStateMessageReceived;
            updateWorker.Dispose();
        }

        public void OnSettingDataSignalRecieve(SettingDataSignal signal)
        {
            if (signal.Command != "OtherNameDisplaySetting")
                return;

            DisplayNameSetting displayNameSetting = (DisplayNameSetting)signal.Data;
            identity.NameText.gameObject.SetActive(displayNameSetting.IsShow);
            identity.BackgroundNameText.gameObject.SetActive(displayNameSetting.IsShow);
            identity.NameText.color = displayNameSetting.Color;
        }

        private void Update()
        {
            if (updateSnapshots.Count <= 0)
                return;

            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.025f);
        }

        private void Tick()
        {
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

        private void OnNameChangeMessageReceived(FakePlayerNameChangeMessage message)
        {
            if (NetworkIdentity.netId != message.NetId)
                return;

            identity.NameText.text = message.Name;
            identity.BackgroundNameText.text = string.Format("<mark=#00000090><alpha=#00>1<space=0em>{0}<space=0em><alpha=#00>1</mark>", message.Name);
        }

        private void OnPartMessageReceived(FakePlayerPartMessage message)
        {
            if (NetworkIdentity.netId != message.NetId)
                return;

            var equipments = new Equipments();
            equipments.hat = message.PartId[0];
            equipments.face = message.PartId[1];
            equipments.head = message.PartId[2];
            equipments.shirt = message.PartId[3];
            equipments.pants = message.PartId[4];
            equipments.shoes = message.PartId[5];
            equipments.back = message.PartId[6];
            avatarSet = new AvatarSet(equipments.JSONForAvatarSet());
            partSwapper.ChangeParts(avatarSet);
        }

        private void OnPositionStateMessageReceived(FakePlayerPositionMessage message)
        {
            if (NetworkIdentity.netId != message.NetId)
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