using UnityEngine;
using Zenject;

namespace com.playbux.ui.draggable
{
    public class SimpleDragUIInstaller : MonoInstaller<SimpleDragUIInstaller>
    {
        [SerializeField]
        private RectTransform dragBar;

        public override void InstallBindings()
        {
            Container.Bind<RectTransform>().FromComponentOn(gameObject).AsSingle();
            Container.BindInterfacesAndSelfTo<SimpleDragUI>().FromNewComponentOn(dragBar.gameObject).AsSingle().NonLazy();
        }
    }
}