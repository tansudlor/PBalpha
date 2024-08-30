using Mirror;
using Zenject;
using UnityEngine;
using UnityEngine.AI;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class MoveSanpoFakePlayerStateInstaller : FakePlayerStateInstaller<MoveSanpoFakePlayerState>
    {
        [Inject]
        private NavMeshAgent navMeshAgent;

        [Inject]
        private NetworkIdentity networkIdentity;

        [SerializeField]
        private GameObjectContext fakePlayerPositionMessageContext;

        [SerializeField]
        private Vector2[] wayPoints;

        public override void AdditionalBindings()
        {
            Container.Bind<NavMeshAgent>().FromInstance(navMeshAgent).AsSingle();
            Container.Bind<NetworkIdentity>().FromInstance(networkIdentity).AsSingle();
            Container.Bind<IServerMessageSender<FakePlayerPositionMessage>>().FromSubContainerResolve().ByNewContextPrefab(fakePlayerPositionMessageContext).AsSingle();
            Container.Bind<Vector2[]>().FromInstance(wayPoints).AsSingle();
        }
    }
}