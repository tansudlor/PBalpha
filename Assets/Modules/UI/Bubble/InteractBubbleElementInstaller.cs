using TMPro;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public class InteractBubbleElementInstaller : MonoInstaller<InteractBubbleElementInstaller>
    {
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private RectTransform rectTransform;

        public override void InstallBindings()
        {
            Container.Bind<InteractBubble>().AsSingle();
            Container.Bind<TextMeshProUGUI>().FromInstance(text).AsSingle();
            Container.Bind<RectTransform>().FromInstance(rectTransform).AsSingle();
        }
    }

}