using Zenject;

namespace com.playbux.ui.gamemenu
{
    public class QuizTimeEventUIInstaller : MonoInstaller<QuizTimeEventUIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<QuizTimeEventUI>().FromComponentOn(gameObject).AsSingle();
        }
    }
}