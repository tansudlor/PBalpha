using Zenject;
using UnityEngine;
using com.playbux.ui;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.client.chat;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.infastructure
{
    [CreateAssetMenu(menuName = "Playbux/Chat/Create ChatInstaller", fileName = "ChatInstaller", order = 0)]
    public partial class ChatInstaller : ScriptableObjectInstaller<ChatInstaller>
    {
        [Inject]
        private UICanvas uiCanvas;

        public override void InstallBindings()
        {
            DiContainer subContainer = Container.CreateSubContainer();
#if SERVER
            BindServer();
#elif !SERVER
            BindClient(Container);
#endif
        }
    }

    public partial class ChatInstaller : ScriptableObjectInstaller<ChatInstaller>
    {
        [SerializeField]
        private ChatServerInstaller chatServerInstaller;

        private void BindServer()
        {
#if SERVER
            Container.Bind<ChatCommandProcessor>().FromSubContainerResolve().ByNewContextPrefab(chatServerInstaller).AsSingle().NonLazy();
#endif
        }
    }

    public partial class ChatInstaller : ScriptableObjectInstaller<ChatInstaller>
    {
        [SerializeField]
        private ChatConfig chatConfig;

        [SerializeField]
        private ChatUIInstaller uiInstaller;

        [SerializeField]
        private ChatPenaltySettings penaltySettings;

        [SerializeField]
        private ChatConstrainSettings constrainSettings;

        private void BindClient(DiContainer subContainer)
        {
#if !SERVER
            subContainer.Bind<ChatConfig>().FromInstance(chatConfig).AsSingle();
            subContainer.Bind<IChatRecorder>().To<SimpleChatRecorder>().AsSingle();
            subContainer.BindInterfacesAndSelfTo<ChatClientBehaviour>().AsSingle();
            subContainer.Bind<ChatPenaltySettings>().FromInstance(penaltySettings).AsSingle();
            subContainer.Bind<ChatConstrainSettings>().FromInstance(constrainSettings).AsSingle();
            subContainer.BindInterfacesAndSelfTo<ClientMessageReceiver<ChatBroadcastMessage>>().AsSingle();
            subContainer.Bind<ChatUIController>().FromSubContainerResolve().ByNewContextPrefab(uiInstaller)
                .UnderTransform(uiCanvas.transform)
                .AsSingle().NonLazy();
#endif
        }
    }

}