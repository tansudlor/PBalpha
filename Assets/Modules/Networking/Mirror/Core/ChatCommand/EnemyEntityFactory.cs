using com.playbux.entities;
using Mirror;
using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.core
{
    public class EnemyEntityFactory : IFactory<GameObject, Vector3, IEntity<EnemyIdentity>>
    {
        private readonly DiContainer container;
        public EnemyEntityFactory(DiContainer container)
        {
            this.container = container;
        }

        public IEntity<EnemyIdentity> Create(GameObject prefab, Vector3 position)
        {
            var facade = container.InstantiatePrefabForComponent<NetworkEnemyFacade>(prefab, position);
#if SERVER
            NetworkServer.Spawn(facade.gameObject);
#endif
            return facade.Entity;
        }
    }

    public class FakePlayerEntityFactory : IFactory<GameObject, Vector3, IEntity<FakePlayerIdentity>>
    {
        private readonly DiContainer container;
        public FakePlayerEntityFactory(DiContainer container)
        {
            this.container = container;
        }

        public IEntity<FakePlayerIdentity> Create(GameObject prefab, Vector3 position)
        {
            var facade = container.InstantiatePrefabForComponent<NetworkFakePlayerFacade>(prefab, position);
#if SERVER
            NetworkServer.Spawn(facade.gameObject);
#endif
            return facade.Entity;
        }
    }
}