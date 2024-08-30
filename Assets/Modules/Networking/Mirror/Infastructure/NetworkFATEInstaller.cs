using Zenject;
using UnityEngine;
using com.playbux.FATE;
using UnityEngine.Serialization;
using com.playbux.networking.mirror.server;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.client.FATE;

namespace com.playbux.networking.mirror.infastructure
{
    public class NetworkFATEInstaller : MonoInstaller<NetworkFATEInstaller>
    {
        [SerializeField]
        private FATEInstaller fateInstaller;

        [SerializeField]
        private FATEDatabase fateDatabase;

        [SerializeField]
        private FATENotificationUIInstaller notificationUIInstaller;
            
        [SerializeField]
        private FATEListUpdateMessageServerSenderInstaller fateListUpdateMessageServerSenderInstaller;
        
        [SerializeField]
        private FATENotificationMessageServerSenderInstaller fateNotificationMessageServerSenderInstaller;

        public override void InstallBindings()
        {
            Container.Bind<FATEDatabase>().FromInstance(fateDatabase).AsSingle();
            
#if SERVER
            Container.BindInterfacesAndSelfTo<FATEListServerUpdater>().AsSingle();
            Container.BindInterfacesAndSelfTo<FATEServerNotifier>().AsSingle().NonLazy();
            Container.Bind<IFATEScheduler>().FromSubContainerResolve().ByNewContextPrefab(fateInstaller).AsSingle();
            Container.Bind<IServerMessageSender<FATEListUpdateMessage>>().FromSubContainerResolve().ByNewContextPrefab(fateListUpdateMessageServerSenderInstaller).AsSingle();
            Container.Bind<IServerMessageSender<FATENotificationMessage>>().FromSubContainerResolve().ByNewContextPrefab(fateNotificationMessageServerSenderInstaller).AsSingle();
#else
            Container.BindInterfacesAndSelfTo<FATEClientListController>().AsSingle();
            Container.BindInterfacesAndSelfTo<FATEClientNotifier>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<FATEListUpdateMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<FATENotificationMessage>>().AsSingle();
            Container.BindFactory<FATENotificationUI, FATENotificationUI.Factory>().FromSubContainerResolve().ByNewContextPrefab(notificationUIInstaller).AsSingle();
#endif
        }
    }
}
