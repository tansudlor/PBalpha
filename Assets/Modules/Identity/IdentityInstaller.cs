using com.playbux.events;
using com.playbux.networking.mirror.message;
using com.playbux.ui;
using UnityEngine;
using Zenject;

namespace com.playbux.identity
{
    [CreateAssetMenu(menuName = "Playbux/Identity/IdentityInstaller", fileName = "IdentityInstaller")]
    public class IdentityInstaller : ScriptableObjectInstaller<IdentityInstaller>
    {
        public GameObject tokenInputPrefab;
        private UICanvas canvas;

        [Inject]
        void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }

        public override void InstallBindings()
        {

#if !SERVER
            BindClientSide();

#endif

#if SERVER
            BindServerSide();
#endif
            Container.Bind<IdentityDataSystem>().AsSingle().NonLazy();

        }

#if SERVER
        private void BindServerSide()
        {
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<UserDataMessage>))
                .To<ServerNetworkMessageReceiver<UserDataMessage>>().AsSingle();


            Container.BindInterfacesAndSelfTo<IdentitySystem>().AsSingle().NonLazy();

            Container.Bind(
                   typeof(IInitializable),
                   typeof(ILateDisposable),
                   typeof(IServerNetworkMessageReceiver<UserProfileMessage>))
               .To<ServerNetworkMessageReceiver<UserProfileMessage>>().AsSingle();

            Container.Bind(
                   typeof(IInitializable),
                   typeof(ILateDisposable),
                   typeof(IServerNetworkMessageReceiver<UserStatusMessage>))
               .To<ServerNetworkMessageReceiver<UserStatusMessage>>().AsSingle();

        }
#endif

#if !SERVER
        private void BindClientSide()
        {
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<UserDataMessage>))
                .To<ClientMessageReceiver<UserDataMessage>>().AsSingle();

            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<UserProfileMessage>))
                .To<ClientMessageReceiver<UserProfileMessage>>().AsSingle();

            Container.BindInterfacesAndSelfTo<IdentitySystem>().AsSingle().NonLazy();


            Container.Bind(
                  typeof(IInitializable),
                  typeof(ILateDisposable),
                  typeof(INetworkMessageReceiver<UserStatusMessage>))
              .To<ClientMessageReceiver<UserStatusMessage>>().AsSingle();


            /* Container.Bind<TokenInput>()
                  .FromComponentInNewPrefab(tokenInputPrefab)
                  .UnderTransform(canvas.transform)
                  .AsSingle();

             Container.BindSignal<AuthenticationSignal>()
                 .ToMethod<TokenInput>(c => c.OnConnectedSignalReceive).FromResolve();*/

            Container.BindSignal<IdentityChangeSignal>().ToMethod<IdentityDataSystem>(c => c.OnIdentityChangeSinal).FromResolve();
        }
#endif
    }
}