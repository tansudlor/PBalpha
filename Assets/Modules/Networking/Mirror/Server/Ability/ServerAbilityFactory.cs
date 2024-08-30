using UnityEngine;
using Zenject;
namespace com.playbux.networking.server.ability
{
    public class ServerAbilityFactory : IFactory<GameObject, Vector2, ServerAbilityFacade>
    {
        private readonly DiContainer container;
        public ServerAbilityFactory(DiContainer container)
        {
            this.container = container;
        }

        public ServerAbilityFacade Create(GameObject prefab, Vector2 position)
        {
            return container.InstantiatePrefabForComponent<ServerAbilityFacade>(prefab, position);
        }
    }
}