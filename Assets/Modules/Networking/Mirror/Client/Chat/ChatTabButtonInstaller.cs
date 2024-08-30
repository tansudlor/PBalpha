using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.client.chat
{
    public class ChatTabButtonInstaller : MonoInstaller<ChatTabButtonInstaller>
    {
        [SerializeField]
        private Transform tabContainer;

        [SerializeField]
        private ChatTab chatTabPrefab;

        [Inject]
        private ChatTabSettings[] settings;

        public override void InstallBindings()
        {
            Container.Bind<Transform>().FromInstance(tabContainer).AsSingle();
            Container.Bind<ChatTabSettings[]>().FromInstance(settings).AsSingle();
            Container.BindInterfacesAndSelfTo<ChatTabButtonController>().AsSingle();
            Container.BindMemoryPool<ChatTab, ChatTab.Pool>()
                .FromComponentInNewPrefab(chatTabPrefab).UnderTransformGroup("TabPool").AsSingle();
        }
    }
}