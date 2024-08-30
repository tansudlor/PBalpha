using Zenject;
using UnityEngine;
using com.playbux.map;

namespace com.playbux.sorting
{
    public class LineSorterInstaller : MonoInstaller<LineSorterInstaller>
    {
        [SerializeField]
        private DepthPivotWrapper pivots;

        public override void InstallBindings()
        {
            Container.Bind<Transform>().FromInstance(transform).AsSingle();
            Container.Bind<DepthPivotWrapper>().FromInstance(pivots).AsSingle();
            Container.Bind<ISortable>().To<LineSortable>().AsSingle().NonLazy();
        }
    }
}