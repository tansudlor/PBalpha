using Zenject;

namespace com.playbux.sorting
{
    public abstract class BaseSortComponentInstaller<T> : MonoInstaller<BaseSortComponentInstaller<T>> where T : ISortComponent
    {
        public override void InstallBindings()
        {
            Container.Bind<ISortComponent>().To<T>().AsSingle();
            BindComponent();
        }

        internal abstract void BindComponent();
    }
}