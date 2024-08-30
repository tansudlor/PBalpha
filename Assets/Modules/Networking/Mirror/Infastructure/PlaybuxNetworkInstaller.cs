using System;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.identity;
using com.playbux.entities;
using UnityEngine.Serialization;
using com.playbux.firebaseservice;
using Object = UnityEngine.Object;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.client;
using com.playbux.networking.mirror.server;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.infastructure
{
    public class PlaybuxNetworkInstaller : MonoInstaller<PlaybuxNetworkInstaller>
    {
        [SerializeField]
        private NetworkSetting startSettings;

        [SerializeField]
        private GameObjectContext playerContext;

        [SerializeField]
        private TransportSettings transportSettings;

        [SerializeField]
        private PlaybuxNetworkManager networkManager;

#if DEBUG_BUILD
        [SerializeField]
        private MirrorPlaybuxNetworkManager debugNetworkManager;
#endif

        public override void InstallBindings()
        {
            //FirebaseRemoteConfigManager.GetInstance();
            Container.BindInterfacesAndSelfTo<NetworkController>().AsSingle().NonLazy();
            Container.Bind<TransportProvider>().AsSingle();
            Container.Bind<NetworkSetting>().FromInstance(startSettings).AsSingle();
            Container.Bind<TransportSettings>().FromInstance(transportSettings).AsSingle();
            Container.Bind<PlaybuxNetworkManager>().FromMethod(GetManager).AsSingle();
#if REMOTE_ENDPOINT
            Container
                .BindFactory<PlaybuxNetworkManager, NetworkSetting, PlaybuxNetworkManager,
                    PlaybuxNetworkManager.Factory>().FromFactory<PlaybuxRemoteNetworkManagerFactory>();
#else
            Container
                .BindFactory<PlaybuxNetworkManager, NetworkSetting, PlaybuxNetworkManager,
                    PlaybuxNetworkManager.Factory>().FromFactory<PlaybuxNetworkManagerFactory>();
#endif

            Container.BindFactory<IEntity<NetworkIdentity>, EntityFactory<NetworkIdentity>>().FromSubContainerResolve().ByNewContextPrefab(playerContext);

            BindServerSide();
            BindClientSide();
        }

        private PlaybuxNetworkManager GetManager()
        {
#if DEBUG_BUILD
            return debugNetworkManager;
#endif

            return networkManager;
        }

        private void BindServerSide()
        {
#if SERVER
            Container.BindInterfacesAndSelfTo<ServerNetworkMessageReceiver<MoveCommandMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServerNetworkMessageReceiver<PlayerMoveInputMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServerNetworkMessageReceiver<TeleportationRequestMessage>>().AsSingle();
#endif
        }

        private void BindClientSide()
        {
#if !SERVER
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<MapDataMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<PlayerListMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<TeleportationValidMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<TeleportationInvalidMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<EntityDespawnMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<MapColliderUpdateMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<EntityEffectUpdateMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<PlayerUpdatePositionMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<MoveCommandValidationMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<OtherPlayerUpdatePositionMessage>>().AsSingle();
#endif
        }
    }
}