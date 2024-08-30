using Zenject;

namespace com.playbux.npc
{
    public class NPCSorterInstaller : MonoInstaller<NPCSorterInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<NPCSorter>().FromComponentOn(gameObject).AsSingle();
        }

    }
}
