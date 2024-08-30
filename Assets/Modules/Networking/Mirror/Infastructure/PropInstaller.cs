using Zenject;
using UnityEngine;
using com.playbux.map;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.collision;
using com.playbux.networking.mirror.client.prop;
using com.playbux.networking.mirror.server.prop;

namespace com.playbux.networking.mirror.infastructure
{
    public class PropInstaller : MonoInstaller<PropInstaller>
    {
        [SerializeField]
        private PropDatabase propDatabase;

        [SerializeField]
        private MapPropDatabase mapPropDatabase;

        [SerializeField]
        private ServerPropColliderUpdateMessageSenderInstaller serverPropColliderUpdateMessageSenderInstaller;

        public override void InstallBindings()
        {
            Container.Bind<PropDatabase>().FromInstance(propDatabase).AsSingle();
            Container.Bind<MapPropDatabase>().FromInstance(mapPropDatabase).AsSingle();
            Container.BindFactory<Object, Transform, Collider2D, Collider2DFactory>().FromFactory<PrefabToCollider2DFactory>();

#if SERVER
            Container.BindInterfacesAndSelfTo<ServerPropController>().AsSingle().NonLazy();
            Container.Bind<IServerMessageSender<PropColliderUpdateMessage>>().FromSubContainerResolve().ByNewContextPrefab(serverPropColliderUpdateMessageSenderInstaller).AsSingle();
#else
            Container.BindInterfacesAndSelfTo<ClientPropController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<PropColliderUpdateMessage>>().AsSingle();
            Container.BindFactory<Object, Vector3, Vector3, ClientProp, ClientProp.Factory>().FromFactory<ClientPropFactory>();
#endif
        }
    }
}