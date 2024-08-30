using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.entities;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.server;
using com.playbux.networking.mirror.client.enemy;
using com.playbux.networking.mirror.server.enemy;

namespace com.playbux.networking.mirror.entity
{
    public abstract class NetworkEnemyEntity : IEntity<EnemyIdentity>
    {
        public uint TargetId => enemyIdentity.Id;
        public GameObject GameObject { get; }
        public EnemyIdentity Identity => enemyIdentity;

        internal readonly EnemyIdentity enemyIdentity;

#if !SERVER
        internal readonly IEnemyClientBehaviour.Factory factory;

        public NetworkEnemyEntity(EnemyIdentity enemyIdentity, IEnemyClientBehaviour.Factory factory, GameObject gameObject)
        {
            GameObject = gameObject;
            this.factory = factory;
            this.enemyIdentity = enemyIdentity;
        }
#else
        internal IEnemyServerBehaviour behaviour;

        public NetworkEnemyEntity(EnemyIdentity enemyIdentity, IEnemyServerBehaviour behaviour, GameObject gameObject)
        {
            GameObject = gameObject;
            this.behaviour = behaviour;
            this.enemyIdentity = enemyIdentity;
        }
#endif

        public void Initialize()
        {
#if DEVELOPMENT
            Debug.Log($"Enemy [{enemyIdentity.Id}]{enemyIdentity.Name} Entity initialized");
#endif

            OnPostInitialize();
        }

        public void Dispose()
        {
            OnPreDispose();

#if DEVELOPMENT
            Debug.Log($"Enemy [{enemyIdentity.Id}]{enemyIdentity.Name} Entity disposed");
#endif
        }

        internal abstract void OnPostInitialize();
        internal abstract void OnPreDispose();


    }
}