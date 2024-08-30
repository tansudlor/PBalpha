using Zenject;
using UnityEngine;
using com.playbux.avatar;
using com.playbux.networking.mirror.message;
using com.playbux.events;


namespace com.playbux.networking.networkavatar
{
    [CreateAssetMenu(menuName = "Playbux/Inventory/AvatarBoardInstaller", fileName = "AvatarBoardInstaller")]
    public class AvatarBoardInstaller : ScriptableObjectInstaller<AvatarBoardInstaller>
    {
        public AvatarSampleSet SampleSet;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NetworkAvatarBoard>().AsSingle().NonLazy(); // Singleton binding
            Container.BindInstance(SampleSet).AsSingle();
#if !SERVER
            BindClientSide();
#endif

#if SERVER
            BindServerSide();
#endif
        }
#if SERVER
        private void BindServerSide()
        {
            // Container.Bind<IEntityBehaviour>().To<AvatarServer>().AsSingle();
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<AvatarUpdateMessage>))
                .To<ServerNetworkMessageReceiver<AvatarUpdateMessage>>().AsSingle();
        }
#endif

#if !SERVER
        private void BindClientSide()
        {
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<AvatarUpdateMessage>))
                .To<ClientMessageReceiver<AvatarUpdateMessage>>().AsSingle();
        }
#endif
    }
}