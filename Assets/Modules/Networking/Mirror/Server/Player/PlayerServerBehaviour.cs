using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.events;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class PlayerServerBehaviour : IServerBehaviour
    {
        private readonly Transform transform;
        private readonly NetworkIdentity networkIdentity;
        private readonly PlayerLayerMaskSettings layerMaskSettings;
        private readonly PlayerServerMoveCommandRecorder commandRecorder;
        private readonly IServerNetworkMessageReceiver<TeleportationRequestMessage> teleportationRequestReceiver;

        private SignalBus signalBus;

        public PlayerServerBehaviour(
            SignalBus signalBus,
            NetworkIdentity networkIdentity,
            PlayerLayerMaskSettings layerMaskSettings,
            PlayerServerMoveCommandRecorder commandRecorder,
            IServerNetworkMessageReceiver<TeleportationRequestMessage> teleportationRequestReceiver)
        {
            this.signalBus = signalBus;
            this.commandRecorder = commandRecorder;
            this.networkIdentity = networkIdentity;
            this.layerMaskSettings = layerMaskSettings;
            this.teleportationRequestReceiver = teleportationRequestReceiver;

            this.signalBus.Subscribe<TeleportationCompleteSignal>(OnTeleportationCompleted);

            transform = this.networkIdentity.transform;

#if DEVELOPMENT
            Debug.Log($"Server player behaviour id {this.networkIdentity.netId} created");
#endif
        }

        public void Initialize()
        {
            commandRecorder.OnColliderTrigger += GetCollisionNext;
            teleportationRequestReceiver.OnEventCalled += OnTeleportRequest;
            commandRecorder.Initialize();

#if DEVELOPMENT
            Debug.Log($"Server player behaviour id {networkIdentity.netId} initialized");
#endif
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<TeleportationCompleteSignal>(OnTeleportationCompleted);

            commandRecorder.Dispose();
            commandRecorder.OnColliderTrigger -= GetCollisionNext;
            teleportationRequestReceiver.OnEventCalled -= OnTeleportRequest;

#if DEVELOPMENT
            Debug.Log($"Server player behaviour id {networkIdentity.netId} disposed");
#endif
        }

        private void OnTeleportRequest(NetworkConnectionToClient connectionToClient, TeleportationRequestMessage message, int channel)
        {
            if (message.NetId != networkIdentity.netId)
                return;

            signalBus.Fire(new TeleportationInitiateSignal(message.NetId, message.KeyPosition, message.ProjectedPosition));
        }

        private void OnTeleportationCompleted(TeleportationCompleteSignal signal)
        {
            if (signal.netId != networkIdentity.netId)
                return;

            commandRecorder.InterruptCommand(signal.teleportedPosition);
        }

        private Vector2 GetCollisionNext(float moveSpeed, Vector2 direction, Vector2 position)
        {
            // Cast the ray equal to amount to move in 1 update tic
            RaycastHit2D hit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.colliderMask.value);
            Vector2 slideDirection = direction;

            if (hit.collider != null)
                slideDirection = Vector2.Reflect(direction, hit.normal);

            hit = Physics2D.Raycast(position, slideDirection, moveSpeed, layerMaskSettings.colliderMask.value);

            if (hit.collider != null)
                slideDirection = Vector2.zero;

            // Check if the ray hit something
            return position + slideDirection * moveSpeed;
        }

        private Vector2 OnTeleportTriggered(float moveSpeed, Vector2 direction, Vector2 position)
        {
            RaycastHit2D teleportHit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.teleportMask.value);
            RaycastHit2D colliderHit = Physics2D.Raycast(position, direction, moveSpeed, layerMaskSettings.colliderMask.value);

            if (colliderHit.collider != null) //NOTE: If hitting the collider before the teleport then we ignore the teleport check completely.
                return position + direction * moveSpeed;

            if (teleportHit.collider == null)
                return position + direction * moveSpeed;

            return position;
        }
    }
}