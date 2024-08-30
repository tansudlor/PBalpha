/*
using Mirror;
using UnityEngine;
using Zenject;
namespace com.playbux.avatar.sample
{
    public class MockEntityTestInstaller : MonoInstaller<MockEntityTestInstaller>
    {
        [SerializeField]
        private NetworkIdentity id;

        [SerializeField]
        private NetworkAvatarController prefab;

        public override void InstallBindings()
        {
            Container.Bind<NetworkIdentity>().FromInstance(id).AsSingle();
            Container.BindInterfacesAndSelfTo<MockEntity>().AsSingle().NonLazy();
            ///create useravatar
            Container.Bind<AvatarController<uint>>().FromSubContainerResolve().ByNewContextPrefab(prefab).AsSingle().NonLazy();
        }
    }
}
*/
