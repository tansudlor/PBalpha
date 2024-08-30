using UnityEngine;
using Zenject;

namespace com.playbux.map
{
    public class CellProviderInstaller : MonoInstaller<CellProviderInstaller>
    {
        [SerializeField]
        private CellInstaller cellInstaller;
        
        public override void InstallBindings()
        {
            Container.Bind<CellProvider>().AsSingle();
            Container.BindMemoryPool<RuntimeCell, RuntimeCell.Pool>().FromSubContainerResolve().ByNewContextPrefab(cellInstaller)
                .AsSingle();
        }
    }
}