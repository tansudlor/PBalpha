using Zenject;
using UnityEngine;
using com.playbux.entities;
using com.playbux.events;
using com.playbux.fakeplayer;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.server.fakeplayer;

namespace com.playbux.networking.mirror.infastructure
{
    public class FakePlayerSystemInstaller : MonoInstaller<FakePlayerSystemInstaller>
    {
        [SerializeField]
        private ServerNavMeshObject serverNavMeshObject;

        [SerializeField]
        private FakePlayerAssetDatabase database;

        public override void InstallBindings()
        {
#if SERVER
            Container.BindInterfacesAndSelfTo<FakePlayerServerSpawner>().AsSingle();
            Container.Bind<ServerNavMeshObject>().FromComponentInNewPrefab(serverNavMeshObject).AsSingle();
            Container.BindSignal<RemoteConfigResponseSignal<int>>().ToMethod<FakePlayerServerSpawner>(spawner => spawner.OnLifeTimeConfigFetched).FromResolveAll();
            Container.BindSignal<RemoteConfigResponseSignal<int>>().ToMethod<FakePlayerServerSpawner>(spawner => spawner.OnMaxAmountConfigFetched).FromResolveAll();
            Container.BindSignal<RemoteConfigResponseSignal<float>>().ToMethod<FakePlayerServerSpawner>(spawner => spawner.OnSpawnTimeConfigFetched).FromResolveAll();
#else
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<FakePlayerPartMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<FakePlayerPositionMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<FakePlayerNameChangeMessage>>().AsSingle();
#endif
            Container.Bind<FakePlayerAssetDatabase>().FromInstance(database).AsSingle();
            Container.BindFactory<GameObject, Vector3, IEntity<FakePlayerIdentity>, NetworkFakePlayerFactory>().FromFactory<FakePlayerEntityFactory>();
        }
    }
}