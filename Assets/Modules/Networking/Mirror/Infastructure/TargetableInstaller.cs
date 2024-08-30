using com.playbux.networking.client.targetable;
using Zenject;
using UnityEngine;
#if SERVER
using com.playbux.networking.server.targetable;
#endif

namespace com.playbux.networking.mirror.infastructure
{
    [CreateAssetMenu(menuName = "Playbux/Targetable/Create TargetableInstaller", fileName = "TargetableInstaller", order = 0)]
    public class TargetableInstaller : ScriptableObjectInstaller<TargetableInstaller>
    {
        public override void InstallBindings()
        {
#if SERVER
            Container.Bind<ServerTargetableController>().AsSingle();
#else
            Container.Bind<ClientTargetableController>().AsSingle();
#endif
        }
    }
}