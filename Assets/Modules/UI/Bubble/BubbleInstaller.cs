using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public class BubbleInstaller : MonoInstaller<BubbleInstaller>
    {
        [SerializeField]
        private BubbleContainerInstaller containerInstaller;
        
        [SerializeField]
        private BubbleElementInstaller bubbleElementInstaller;

        [SerializeField]
        private InteractBubbleElementInstaller interactBubbleElementInstaller;

        public override void InstallBindings()
        {
            Container.Bind<BubbleController<Bubble.Pool>>().AsSingle();
            //Container.Bind<BubbleController<InteractBubble.Pool>>().AsSingle();
            Container.Bind<BubbleContainer>().FromSubContainerResolve().ByNewContextPrefab(containerInstaller).AsSingle();
            Container.BindMemoryPool<Bubble, Bubble.Pool>().FromSubContainerResolve().ByNewContextPrefab(bubbleElementInstaller).AsSingle();
            //Container.BindMemoryPool<InteractBubble, InteractBubble.Pool>().FromSubContainerResolve().ByNewContextPrefab(interactBubbleElementInstaller).AsCached();
        }
    }
}