using Zenject;
using UnityEngine;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.server.chat;

namespace com.playbux.networking.mirror.infastructure
{
    public class ChatServerInstaller : MonoInstaller<ChatServerInstaller>
    {
        [SerializeField]
        private GameObjectContext[] chatCommandInstallers;

        public override void InstallBindings()
        {
#if SERVER
            Container.Bind<ChatCommandProcessor>().AsSingle();
            Container.BindInterfacesAndSelfTo<ChatSeverBehaviour>().AsSingle();
            Container.BindInterfacesAndSelfTo<ServerNetworkMessageReceiver<ChatCommandMessage>>().AsSingle();

            foreach (var installer in chatCommandInstallers)
            {
                Container.Bind<ICommandWorker>().FromSubContainerResolve().ByNewContextPrefab(installer).AsCached();
            }
#endif
        }
    }
}