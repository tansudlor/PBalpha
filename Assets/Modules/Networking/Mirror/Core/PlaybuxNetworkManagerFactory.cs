using Zenject;
using com.playbux.firebaseservice;
using Cysharp.Threading.Tasks;
namespace com.playbux.networking.mirror.core
{
    public class PlaybuxNetworkManagerFactory : IFactory<PlaybuxNetworkManager, NetworkSetting, PlaybuxNetworkManager>
    {
        private DiContainer container;
        private TransportProvider transportProvider;

        public PlaybuxNetworkManagerFactory(DiContainer container, TransportProvider transportProvider)
        {
            this.container = container;
            this.transportProvider = transportProvider;
        }

        public PlaybuxNetworkManager Create(PlaybuxNetworkManager prefab, NetworkSetting settings)
        {
            prefab.networkAddress = settings.networkAddress;
            prefab.transport = transportProvider.Get(settings.transportName);
            return container.InstantiatePrefabForComponent<PlaybuxNetworkManager>(prefab);
        }
    }


    public class PlaybuxRemoteNetworkManagerFactory : IFactory<PlaybuxNetworkManager, NetworkSetting, PlaybuxNetworkManager>
    {
        private DiContainer container;
        private TransportProvider transportProvider;

        public PlaybuxRemoteNetworkManagerFactory(DiContainer container, TransportProvider transportProvider)
        {
            this.container = container;
            this.transportProvider = transportProvider;
        }

        public PlaybuxNetworkManager Create(PlaybuxNetworkManager prefab, NetworkSetting settings)
        {
            
            return container.InstantiatePrefabForComponent<PlaybuxNetworkManager>(prefab);
        }
    }
}
