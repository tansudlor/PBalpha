using UnityEngine;
using Zenject;
namespace com.playbux.networking.client.ability
{
    public class ClientAbilityFactory : IFactory<Object, Vector3, IClientAbility>
    {
        private readonly DiContainer container;

        public ClientAbilityFactory(DiContainer container)
        {
            this.container = container;
        }

        public IClientAbility Create(Object prefab, Vector3 position)
        {
            return container.InstantiatePrefabForComponent<IClientAbility>(prefab, position);
        }
    }
}
