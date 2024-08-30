using Zenject;
using UnityEngine;
using com.playbux.map;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.collision;
using com.playbux.networking.mirror.client.map;
using com.playbux.networking.mirror.server.map;

namespace com.playbux.networking.mirror.infastructure
{

    public class MapInstaller : MonoInstaller<MapInstaller>
    {
        [SerializeField]
        private MapDatabase database;

        [SerializeField]
        private MapColliderDatabase colliderDatabase;

        [SerializeField]
        private CellProviderInstaller cellProviderInstaller;

        [SerializeField]
        private MapDataMessageSenderInstaller mapDataMessageServerSender;

        [SerializeField]
        private MapColliderMessageSenderInstaller mapColliderMessageSenderInstaller;

        public override void InstallBindings()
        {
            Container.Bind<MapDatabase>().FromInstance(database).AsSingle();

#if SERVER
            Container.Bind<IMapController>().To<ServerMapController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ServerMapColliderController>().AsSingle();
            Container.Bind<IServerMessageSender<MapDataMessage>>().FromSubContainerResolve().ByNewContextPrefab(mapDataMessageServerSender).AsSingle();
            Container.Bind<IServerMessageSender<MapColliderUpdateMessage>>().FromSubContainerResolve().ByNewContextPrefab(mapColliderMessageSenderInstaller).AsSingle();
#else
            Container.BindInterfacesAndSelfTo<ClientMapController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ClientMapColliderController>().AsSingle();
            Container.Bind<CellProvider>().FromSubContainerResolve().ByNewContextPrefab(cellProviderInstaller).AsSingle();
#endif

            Container.Bind<MapColliderDatabase>().FromInstance(colliderDatabase).AsSingle();
        }
    }
}