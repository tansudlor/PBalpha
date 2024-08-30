using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui.sortable
{
    public class SortableUIInstaller : MonoInstaller<SortableUIInstaller>
    {
        [SerializeField]
        private UILayerGroup uiLayerGroup;

        [SerializeField]
        private Canvas sortingCanvas;

        [SerializeField]
        private Button[] bringToTopAreas;

        public override void InstallBindings()
        {
            Container.Bind<UILayerGroup>().FromInstance(uiLayerGroup).AsSingle();
            Container.Bind<Canvas>().FromInstance(sortingCanvas).AsSingle();
            Container.Bind<Button[]>().FromInstance(bringToTopAreas).AsSingle();
            Container.BindInterfacesAndSelfTo<SortableUI>().AsSingle();
        }
    }
}