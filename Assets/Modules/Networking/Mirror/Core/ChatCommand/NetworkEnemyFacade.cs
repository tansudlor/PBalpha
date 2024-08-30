using Zenject;
using UnityEngine;
using com.playbux.entities;

namespace com.playbux.networking.mirror.core
{
    public class NetworkEnemyFacade : MonoBehaviour
    {
        public IEntity<EnemyIdentity> Entity => entity;

        private IEntity<EnemyIdentity> entity;

        [Inject]
        private void Construct(IEntity<EnemyIdentity> entity)
        {
            this.entity = entity;
        }
    }

}