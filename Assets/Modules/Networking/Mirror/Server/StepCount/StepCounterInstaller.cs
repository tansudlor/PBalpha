using Zenject;
namespace com.playbux.networking.server.stepcount
{
    public class StepCounterInstaller : MonoInstaller<StepCounterInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<StepCounter>().AsSingle();
        }
    }
}