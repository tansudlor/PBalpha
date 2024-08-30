using Zenject;

namespace com.playbux.networking.mirror.client.prop
{
    public class ClientPropInstaller : MonoInstaller<ClientPropInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ClientProp>().FromComponentOn(gameObject).AsSingle();
        }
    }
}