using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.client.building
{
    public class PrefabBuildingFactory : IFactory<GameObject, Vector3, IBuilding>
    {
        private readonly DiContainer container;

        public PrefabBuildingFactory(DiContainer container)
        {
            this.container = container;
        }

        public IBuilding Create(GameObject prefab, Vector3 position)
        {
            return container.InstantiatePrefabForComponent<IBuilding>(prefab, position, prefab.transform.rotation, null);
        }
    }
}