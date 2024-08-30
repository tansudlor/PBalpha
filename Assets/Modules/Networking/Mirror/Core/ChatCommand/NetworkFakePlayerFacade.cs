using Zenject;
using UnityEngine;
using com.playbux.entities;

namespace com.playbux.networking.mirror.core
{
    public class NetworkFakePlayerFacade : MonoBehaviour
    {
        public IEntity<FakePlayerIdentity> Entity => entity;

        private IEntity<FakePlayerIdentity> entity;

        [Inject]
        private void Construct(IEntity<FakePlayerIdentity> entity)
        {
            this.entity = entity;
        }
    }
}