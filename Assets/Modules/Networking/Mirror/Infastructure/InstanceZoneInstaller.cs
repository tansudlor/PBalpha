using Zenject;
using UnityEngine;
using com.playbux.zone;
using om.playbux.networking.mirror.client.zone;
using om.playbux.networking.mirror.server.zone;

namespace com.playbux.networking.mirror.infastructure
{
    public class InstanceZoneInstaller : MonoInstaller<InstanceZoneInstaller>
    {
        [SerializeField]
        private ZoneDatabase database;

        public override void InstallBindings()
        {
            Container.Bind<ZoneDatabase>().FromInstance(database).AsSingle();

#if SERVER
            Container.BindInterfacesAndSelfTo<InstanceServerZoneController>().AsSingle().NonLazy();
#else
            Container.BindInterfacesAndSelfTo<InstanceClientZoneController>().AsSingle().NonLazy();
#endif
        }
    }
}