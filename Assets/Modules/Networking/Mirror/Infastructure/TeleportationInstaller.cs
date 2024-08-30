using Zenject;
using UnityEngine;
using com.playbux.networking.mirror.core;
using com.playbux.network.mirror.client.telelportation;
using com.playbux.networking.mirror.server.teleportation;

namespace com.playbux.networking.mirror.infastructure
{
    [CreateAssetMenu(menuName = "Playbux/Teleportation/Create TeleportationInstaller", fileName = "TeleportationInstaller", order = 0)]
    public class TeleportationInstaller : ScriptableObjectInstaller<TeleportationInstaller>
    {
        [SerializeField]
        private TeleportPointDatabase database;

        public override void InstallBindings()
        {
#if SERVER
            Container.BindInterfacesAndSelfTo<ServerTeleportationController>().AsSingle().NonLazy();
#else
            Container.BindInterfacesAndSelfTo<ClientTeleportationController>().AsSingle().NonLazy();
#endif
            Container.Bind<TeleportPointDatabase>().FromInstance(database).AsSingle();
            Container.BindFactory<Vector2, TeleportableArea, TeleportableArea, TeleportableArea.Factory>().FromFactory<TeleportableAreaFactory>();
        }
    }
}