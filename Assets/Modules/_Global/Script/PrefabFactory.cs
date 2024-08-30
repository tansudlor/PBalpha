using UnityEngine;
using Zenject;

namespace com.playbux
{
    public class PrefabFactory : PlaceholderFactory<GameObject, Transform, GameObject>
    {
        public override GameObject Create(GameObject param1, Transform param2 = null)
        {
            return base.Create(param1, param2);
        }
    }

    public class PrefabFactory<T> : PlaceholderFactory<Object, Transform, T> where T : Component
    {
        public override T Create(Object param1, Transform param2 = null)
        {
            return base.Create(param1, param2);
        }
    }

    public class PrefabToGameObjectFactory : IFactory<GameObject, Transform, GameObject>
    {
        private readonly DiContainer container;

        public PrefabToGameObjectFactory(DiContainer container)
        {
            this.container = container;
        }

        public GameObject Create(GameObject prefab, Transform parent = null)
        {
            return parent == null ? container.InstantiatePrefab(prefab) : container.InstantiatePrefab(prefab, parent);
        }
    }

    public abstract class PrefabToComponentFactory<T> : IFactory<Object, Transform, T> where T : Component
    {
        private readonly DiContainer container;

        public PrefabToComponentFactory(DiContainer container)
        {
            this.container = container;
        }

        public T Create(Object prefab, Transform parent = null)
        {
            var instance = parent == null ? container.InstantiatePrefabForComponent<T>(prefab) : container.InstantiatePrefabForComponent<T>(prefab, parent);
            OnCreated(instance);
            return instance;
        }

        public abstract void OnCreated(T component);
    }
}