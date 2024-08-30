
using com.playbux.input;
using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.avatar.sample
{
    [CreateAssetMenu(menuName = "Playbux/Avatar/AvatarTestInstaller", fileName = "AvatarTestInstaller")]
    public class AvatarTestInstaller : ScriptableObjectInstaller<AvatarTestInstaller>
    {
        [SerializeField]
        private GameObjectContext mockEntityPrefab;

        public override void InstallBindings()
        {
            //Container.Bind<PlayerControls>().AsSingle();
            //Container.BindInterfacesAndSelfTo<AvatarTestSceneController>().AsSingle();
            //     Container.BindInterfacesAndSelfTo<PlayerMovementInputController>().AsSingle();
            //     Container.BindFactory<MockEntity, MockEntity.Factory>().FromSubContainerResolve().ByNewContextPrefab(mockEntityPrefab);
        }
    }

    public class MockEntity : IInitializable
    {

        IAvatarBoard<uint, NetworkIdentity> board;
        NetworkIdentity id;
        public MockEntity(NetworkIdentity id, IAvatarBoard<uint, NetworkIdentity> board)
        {
            this.board = board;
            this.id = id;
        }

        public void Initialize()
        {
            //    board.UpdateAvatarDirection(id, 0);
        }

        public class Factory : PlaceholderFactory<MockEntity>
        {

        }
    }

    public class AvatarTestSceneController : IInitializable
    {
        private readonly MockEntity.Factory factory;

        public AvatarTestSceneController(MockEntity.Factory factory)
        {
            this.factory = factory;
        }

        public void Initialize()
        {
            factory.Create();
        }
    }
}
