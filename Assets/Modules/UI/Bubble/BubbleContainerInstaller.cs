using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public class BubbleContainerInstaller : MonoInstaller<BubbleContainerInstaller>
    {
        [SerializeField]
        private Canvas canvas;
        
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RectTransform rectTransform;

        [SerializeField]
        private Transform[] channels;

        public override void InstallBindings()
        {
            Container.Bind<BubbleContainer>().AsSingle();
            Container.Bind<Canvas>().FromInstance(canvas).AsSingle();
            Container.Bind<Transform[]>().FromInstance(channels).AsSingle();
            Container.Bind<CanvasGroup>().FromInstance(canvasGroup).AsSingle();
            Container.Bind<RectTransform>().FromInstance(rectTransform).AsSingle();
        }
    }
}