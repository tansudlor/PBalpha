using Zenject;
using UnityEngine;
using com.playbux.map;

namespace com.playbux.networking.mirror.client.building
{
    public class ClientBuildingInstaller : MonoInstaller<ClientBuildingInstaller>
    {
        [SerializeField]
        private MapBuildingDatabase database;

        public override void InstallBindings()
        {
#if !SERVER
            Container.Bind<MapBuildingDatabase>().FromInstance(database).AsSingle();
            Container.BindInterfacesAndSelfTo<ClientBuildingController>().AsSingle();
            Container.BindFactory<GameObject, Vector3, IBuilding, IBuilding.Factory>().FromFactory<PrefabBuildingFactory>();
#endif
        }
    }
}