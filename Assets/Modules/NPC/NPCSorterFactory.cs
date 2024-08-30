using UnityEngine;
using Zenject;

namespace com.playbux.npc
{
    public class NPCSorterFactory : IFactory<GameObject, Vector3, NPCSorter>
    {
        private readonly DiContainer container;

        public NPCSorterFactory(DiContainer container)
        {
            this.container = container;
        }

        public NPCSorter Create(GameObject prefab, Vector3 position)
        {
            var instance = container.InstantiatePrefabForComponent<NPCSorter>(prefab, position, prefab.transform.rotation, null);
            return instance;
        }
    }
}