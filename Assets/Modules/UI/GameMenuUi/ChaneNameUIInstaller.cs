using Zenject;

namespace com.playbux.ui.gamemenu
{
    public class ChaneNameUIInstaller : MonoInstaller<ChaneNameUIInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChangeNameUIController>().FromComponentOn(gameObject).AsSingle();
        }
    }
}