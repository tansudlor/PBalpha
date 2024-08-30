using TMPro;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public class BubbleElementInstaller : MonoInstaller<BubbleElementInstaller>
    {
        [SerializeField]
        private TextMeshProUGUI text;
        
        [SerializeField]
        private RectTransform rectTransform;

        public override void InstallBindings()
        {
            Container.Bind<Bubble>().AsSingle();
            Container.Bind<TextMeshProUGUI>().FromInstance(text).AsSingle();
            Container.Bind<RectTransform>().FromInstance(rectTransform).AsSingle();
        }
    }

}