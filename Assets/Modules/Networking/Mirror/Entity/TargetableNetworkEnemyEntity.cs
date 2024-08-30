using UnityEngine;
using com.playbux.networking.mirror.core;
using com.playbux.networking.client.targetable;
using com.playbux.networking.mirror.client.enemy;
using com.playbux.networking.mirror.server.enemy;
using com.playbux.networking.server.targetable;

namespace com.playbux.networking.mirror.entity
{
    public class TargetableNetworkEnemyEntity : NetworkEnemyEntity
    {
#if !SERVER
        private readonly ClientTargetableController targetableController;

        private IEnemyClientBehaviour enemyClientBehaviour;

        public TargetableNetworkEnemyEntity(
            GameObject gameObject,
            EnemyIdentity enemyIdentity,
            IEnemyClientBehaviour.Factory factory,
            ClientTargetableController targetableController)
            : base(enemyIdentity, factory, gameObject)
        {
            this.targetableController = targetableController;
        }

        internal override void OnPostInitialize()
        {
            enemyClientBehaviour = factory.Create(enemyIdentity);
            targetableController.Add(enemyIdentity.Id, enemyIdentity.NetworkIdentity.transform.position);
        }

        internal override void OnPreDispose()
        {
            enemyClientBehaviour.Dispose();
            targetableController.Remove(enemyIdentity.Id);
        }
#else
        private ServerTargetableController targetableController;

        public TargetableNetworkEnemyEntity(
            GameObject gameObject,
            EnemyIdentity enemyIdentity,
            IEnemyServerBehaviour behaviour,
            ServerTargetableController targetableController)
            : base(enemyIdentity, behaviour, gameObject)
        {
            this.targetableController = targetableController;
        }

        internal override void OnPostInitialize()
        {
            behaviour.Initialize();
            targetableController.Add(enemyIdentity.Id, enemyIdentity.NetworkIdentity.transform.position);
        }

        internal override void OnPreDispose()
        {
            behaviour.Dispose();
            targetableController.Remove(enemyIdentity.Id);
        }
#endif
    }
}