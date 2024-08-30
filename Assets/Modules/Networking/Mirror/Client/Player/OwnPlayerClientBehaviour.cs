using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.ui;
using com.playbux.map;
using com.playbux.input;
using com.playbux.Camera;
using com.playbux.events;
using com.playbux.sorting;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using PlayMode = SETHD.Echo.PlayMode;
using com.playbux.networking.mirror.core;
using com.playbux.networking.networkavatar;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client
{
    public class OwnPlayerClientBehaviour : BasePlayerClientBehaviour
    {
        private const string BGM_OUTSIDE = "BGM/Exterior";
        private const string BGM_INSIDE = "BGM/Interior";

        private readonly ICamera camera;
        private readonly SignalBus signalBus;
        private readonly TeleportScreenCutout cutout;
        private readonly IMapController mapController;
        private readonly IInputController<Vector2> inputController;
        private readonly PlayerLayerMaskSettings layerMaskSettings;
        private readonly PlayerClientMoveCommandRecorder clientMoveCommandRecorder;
        private readonly INetworkMessageReceiver<TeleportationValidMessage> teleportationValidReceiver;
        private readonly INetworkMessageReceiver<TeleportationInvalidMessage> teleportationInvalidReceiver;

        public OwnPlayerClientBehaviour(
            ICamera camera,
            SignalBus signalBus,
            NetworkAvatarBoard board,
            SortingGroup sortingGroup,
            TeleportScreenCutout cutout,
            IMapController mapController,
            NetworkIdentity networkIdentity,
            IInputController<Vector2> inputController,
            PlayerLayerMaskSettings layerMaskSettings,
            LayerSorterController layerSorterController,
            PlayerClientMoveCommandRecorder clientMoveCommandRecorder,
            INetworkMessageReceiver<TeleportationValidMessage> teleportationValidReceiver,
            INetworkMessageReceiver<TeleportationInvalidMessage> teleportationInvalidReceiver)
            : base(board, sortingGroup, networkIdentity, layerSorterController)
        {
            this.cutout = cutout;
            this.camera = camera;
            this.signalBus = signalBus;
            this.mapController = mapController;
            this.inputController = inputController;
            this.layerMaskSettings = layerMaskSettings;
            this.clientMoveCommandRecorder = clientMoveCommandRecorder;
            this.teleportationValidReceiver = teleportationValidReceiver;
            this.teleportationInvalidReceiver = teleportationInvalidReceiver;

#if DEVELOPMENT
            Debug.Log("Client player behaviour created");
#endif
        }

        public override void Initialize()
        {
            inputController.OnHold += HandleAnimation;
            inputController.OnReleased += StopAnimation;
            clientMoveCommandRecorder.OnMove += HandleSortingOrder;
            clientMoveCommandRecorder.OnColliderTrigger += GetCollisionNext;
            clientMoveCommandRecorder.OnTeleportTrigger += OnTeleportTriggered;
            teleportationValidReceiver.OnEventCalled += OnTeleportValidMessageReceived;
            teleportationInvalidReceiver.OnEventCalled += OnTeleportInvalidMessageReceived;

            inputController.Enable();
            clientMoveCommandRecorder.Initialize();

            HandleSortingOrder();
            camera.Follow(NetworkIdentity.transform);
        }

        public override void Dispose()
        {
            inputController.OnHold -= HandleAnimation;
            inputController.OnReleased -= StopAnimation;
            clientMoveCommandRecorder.OnMove -= HandleSortingOrder;
            clientMoveCommandRecorder.OnColliderTrigger -= GetCollisionNext;
            clientMoveCommandRecorder.OnTeleportTrigger -= OnTeleportTriggered;
            teleportationValidReceiver.OnEventCalled -= OnTeleportValidMessageReceived;
            teleportationInvalidReceiver.OnEventCalled -= OnTeleportInvalidMessageReceived;
            inputController.Disable();
            clientMoveCommandRecorder.Dispose();
        }

        private void HandleAnimation(Vector2 direction)
        {
            HandleWalkAnimation(1, direction);
        }

        private void StopAnimation()
        {
            HandleWalkAnimation(1, Vector2.zero);
        }

        public void OnTeleportInvalidMessageReceived(TeleportationInvalidMessage validMessage)
        {
            if (validMessage.NetId != NetworkIdentity.netId)
                return;

            cutout.FadeOut(NetworkIdentity.transform).Forget();
            clientMoveCommandRecorder.EnablePolling(true);
        }

        private void OnTeleportValidMessageReceived(TeleportationValidMessage validMessage)
        {
            if (validMessage.NetId != NetworkIdentity.netId)
                return;

            LockTeleport(validMessage.IsInside, validMessage.TargetPosition).Forget();
        }

        private async UniTaskVoid LockTeleport(bool isInside, Vector2 teleportPosition)
        {
            string bgmName = isInside ? BGM_INSIDE : BGM_OUTSIDE;
            signalBus.Fire(new BGMPlaySignal(bgmName, PlayMode.Transit));
            await UniTask.WaitForSeconds(1f);
            NetworkIdentity.transform.position = teleportPosition;
            HandleSortingOrder();
            await UniTask.WaitForSeconds(0.5f);
            await cutout.FadeOut(NetworkIdentity.transform);
            clientMoveCommandRecorder.EnablePolling(true);
        }

        private Vector2 GetCollisionNext(float moveSpeed, Vector2 direction, Vector2 position)
        {
            // Cast the ray equal to amount to move in 1 update tic
            RaycastHit2D hit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.colliderMask.value);
            Vector2 slideDirection = direction;

#if UNITY_EDITOR || DEBUG
            Debug.DrawRay(position, direction * moveSpeed, Color.green, 1f);
#endif

            if (hit.collider != null)
                slideDirection = Vector2.Reflect(direction, hit.normal);

            hit = Physics2D.Raycast(position, slideDirection, moveSpeed, layerMaskSettings.colliderMask.value);

            if (hit.collider != null)
                slideDirection = Vector2.zero;

            // Check if the ray hit something
            return position + (slideDirection * moveSpeed);
        }

        private Vector2 OnTeleportTriggered(float moveSpeed, Vector2 direction, Vector2 position)
        {
            RaycastHit2D teleportHit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.teleportMask.value);
            RaycastHit2D colliderHit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.colliderMask.value);

            if (colliderHit.collider != null) //NOTE: If hitting the collider before the teleport then we ignore the teleport check completely.
                return position + direction * moveSpeed;

            if (teleportHit.collider == null)
                return position + direction * moveSpeed;

            cutout.FadeIn(NetworkIdentity.transform);
            clientMoveCommandRecorder.EnablePolling(false);
            NetworkClient.connection.Send(new TeleportationRequestMessage(NetworkIdentity.netId, teleportHit.transform.position, position + direction * moveSpeed));
            return position;
        }
    }
}