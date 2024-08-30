using Zenject;

namespace com.playbux.networkquest
{
    public class QuestHelperWindowInstaller : MonoInstaller<QuestHelperWindowInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<QuestHelperWindow>().FromComponentOn(gameObject).AsSingle();
        }
    }
}