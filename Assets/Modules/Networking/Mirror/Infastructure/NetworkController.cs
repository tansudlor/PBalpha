using Mirror;
using Zenject;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.infastructure
{
    public class NetworkController : IInitializable
    {
        private PlaybuxNetworkManager manager;

        private readonly PlaybuxNetworkManager prefab;
        private readonly TransportProvider transportProvider;
        private readonly PlaybuxNetworkManager.Factory factory;

        [InjectOptional]
        private readonly NetworkSetting defaultNetworkSettings = new NetworkSetting("localhost");

        public PlaybuxNetworkManager Manager { get => manager;  }

        public NetworkController(PlaybuxNetworkManager prefab, TransportProvider transportProvider, PlaybuxNetworkManager.Factory factory)
        {
            this.prefab = prefab;
            this.factory = factory;
            this.transportProvider = transportProvider;
        }

        public void Initialize()
        {
            manager = factory.Create(prefab, defaultNetworkSettings);
#if SERVER
            manager.StartServer();
#else
            NetworkClient.OnConnectedEvent += OnClientConnected;
            //manager.StartClient();
#endif
        }

#if !SERVER
        private void OnClientConnected()
        {
            // await UniTask.WaitUntil(() => NetworkClient.isConnected);

            if (NetworkClient.ready)
                return;

            NetworkClient.Ready();

            if (NetworkClient.localPlayer != null)
                return;

            NetworkClient.AddPlayer();
        }
#endif
    }
}