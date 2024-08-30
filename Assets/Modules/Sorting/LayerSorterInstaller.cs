using Zenject;
namespace com.playbux.sorting
{
    public class LayerSorterInstaller : MonoInstaller<LayerSorterInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<LayerSorterController>().AsSingle();
        }
    }
}