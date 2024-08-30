using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.client.prop
{
    public class ClientPropFactory : IFactory<Object, Vector3, Vector3, ClientProp>
    {
        private readonly DiContainer container;

        public ClientPropFactory(DiContainer container)
        {
            this.container = container;
        }

        public ClientProp Create(Object prefab, Vector3 position, Vector3 scale)
        {
            var instance = container.InstantiatePrefabForComponent<ClientProp>(prefab, position, Quaternion.identity, null);
            instance.transform.localScale = scale;
            return instance;
        }
    }
}