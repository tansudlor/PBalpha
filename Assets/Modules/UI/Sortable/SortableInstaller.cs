using Zenject;
using UnityEngine;

namespace com.playbux.ui.sortable
{
    public class SortableInstaller : MonoInstaller<SortableInstaller>
    {
        [SerializeField]
        private UILayerSettings settings;

        public override void InstallBindings()
        {
            Container.Bind<SortableController>().AsSingle();
            Container.Bind<UILayerSettings>().FromInstance(settings).AsSingle();
        }
    }
}