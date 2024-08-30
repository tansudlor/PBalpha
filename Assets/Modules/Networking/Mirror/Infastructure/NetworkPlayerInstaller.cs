using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.bux;
using com.playbux.input;
using com.playbux.motor;
using com.playbux.avatar;
using com.playbux.entities;
using UnityEngine.Rendering;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.entity;
using com.playbux.networking.mirror.message;

#if !SERVER
using com.playbux.networking.mirror.client;
using com.playbux.networking.mirror.server;
#endif

#if SERVER
using com.playbux.networking.mirror.server;
#endif

namespace com.playbux.networking.mirror.infastructure
{
    public class NetworkPlayerInstaller : MonoInstaller<NetworkPlayerInstaller>
    {
        [SerializeField]
        private bool autoPlayer;

        [SerializeField]
        private SortingGroup sortingGroup;

        [SerializeField]
        private PlayerLayerMaskSettings layerMaskSettings;

        [SerializeField]
        private GameObjectContext clientPlayerMoveTickSenderContext;

        [SerializeField]
        private GameObjectContext playerMoveCommandTickSenderContext;

        [SerializeField]
        private GameObjectContext serverOtherPlayerPositionStateTickSenderContext;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SteerMotor>().AsSingle();
            Container.Bind<PlayerLayerMaskSettings>().FromInstance(layerMaskSettings).AsSingle();
            Container.Bind<IEntity<NetworkIdentity>>().To<NetworkPlayerEntity>().AsSingle();
            Container.BindInterfacesAndSelfTo<InternalUpdateWorker>().AsSingle();
            Container.Bind<NetworkIdentity>().FromComponentOn(gameObject).AsSingle();
            var networkIdentity = gameObject.GetComponent<NetworkIdentity>();
            Container.Bind<uint>().FromInstance(networkIdentity.netId).AsSingle();
#if !SERVER
            if (autoPlayer)
                Container.BindInterfacesAndSelfTo<AutoInputWorker>().AsSingle();
            else
                Container.BindInterfacesAndSelfTo<AFKWorker>().AsSingle();

            Container.Bind<SortingGroup>().FromInstance(sortingGroup).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerMovementInputController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerClientMoveCommandRecorder>().AsSingle();
            Container.BindFactory<bool, IClientBehaviour, IClientBehaviour.Factory>().FromFactory<PlayerClientBehaviourFactory>();
            Container.Bind<IClientMessageSender<MoveCommandMessage>>().FromSubContainerResolve().ByNewContextPrefab(playerMoveCommandTickSenderContext).AsTransient();
            
#endif
#if SERVER
            Container.BindInterfacesAndSelfTo<PlayerServerBehaviour>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerServerMoveCommandRecorder>().AsSingle();
            Container.Bind<IServerMessageSender<OtherPlayerUpdatePositionMessage>>().FromSubContainerResolve().ByNewContextPrefab(serverOtherPlayerPositionStateTickSenderContext).AsSingle();
            
#endif
            Container.Bind<Transform>().FromInstance(transform).AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
        }
    }
}