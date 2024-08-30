using Zenject;
using UnityEngine;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.enemy
{
    public interface IEnemyClientBehaviour
    {
        void Initialize(EnemyIdentity enemyIdentity);

        void Dispose();

        void ChangeDirection(Vector2 turnDirection);

        public class Factory : PlaceholderFactory<EnemyIdentity, IEnemyClientBehaviour>
        {

        }
    }

    public class MadBuxEnemyClientBehaviourFactory : IFactory<EnemyIdentity, IEnemyClientBehaviour>
    {
        private readonly DiContainer container;

        public MadBuxEnemyClientBehaviourFactory(DiContainer container)
        {
            this.container = container;
        }

        public IEnemyClientBehaviour Create(EnemyIdentity enemyIdentity)
        {
            var behaviour = container.Instantiate<MadBuxEnemyClientBehaviour>();
            behaviour.Initialize(enemyIdentity);
            return behaviour;
        }
    }
}