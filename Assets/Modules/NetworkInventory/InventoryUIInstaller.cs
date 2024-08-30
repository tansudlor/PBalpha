using Zenject;
using UnityEngine;
using com.playbux.input;
using com.playbux.ui.sortable;

namespace com.playbux.networking.networkinventory
{
    public class InventoryUIInstaller : MonoInstaller<InventoryUIInstaller>
    {
        [SerializeField]
        private InventoryUIController uiController;

        public override void InstallBindings()
        {
            Container.Bind<InventoryUIController>().FromInstance(uiController).AsSingle();
            Container.Bind<IInputController>().To<InventoryInputController>().AsSingle();
        }
    }
}
