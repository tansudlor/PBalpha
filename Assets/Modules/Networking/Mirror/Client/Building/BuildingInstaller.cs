using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.client.building
{
    public class BuildingInstaller : MonoInstaller<BuildingInstaller>
    {
        [SerializeField]
        private PrefabBuilding buildingComponent;

        public override void InstallBindings()
        {
            Container.Bind<IBuilding>().FromInstance(buildingComponent).AsSingle();
        }
    }
}