using Zenject;
using UnityEngine;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class RandomEquipmentFakePlayerStateInstaller : FakePlayerStateInstaller<RandomEquipmentFakePlayerState>
    {
        [Inject]
        private FakePlayerIdentity fakePlayerIdentity;

        [SerializeField]
        private GameObjectContext fakePlayerPartMessageContext;

        public override void AdditionalBindings()
        {
            Container.Bind<FakePlayerIdentity>().FromInstance(fakePlayerIdentity).AsSingle();
            Container.Bind<IServerMessageSender<FakePlayerPartMessage>>().FromSubContainerResolve().ByNewContextPrefab(fakePlayerPartMessageContext).AsSingle();
        }
    }
}