using UnityEngine;
using Zenject;
using com.playbux.networking.mirror.message;

namespace com.playbux.versioning
{
    [CreateAssetMenu(menuName = "Playbux/Versioning/VersioningInstaller", fileName = "VersioningInstaller")]
    public class VersioningInstaller : ScriptableObjectInstaller<VersioningInstaller>
    {
        [SerializeField]
        private Versioning versioning;

        public override void InstallBindings()
        {
            Container.Bind<Versioning>().FromInstance(versioning).AsSingle().NonLazy();
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



            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<VersioningInfo>))
                .To<ServerNetworkMessageReceiver<VersioningInfo>>().AsSingle();



        }
#endif

#if !SERVER
        private void BindClientSide()
        {
          


            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<VersioningInfo>))
                .To<ClientMessageReceiver<VersioningInfo>>().AsSingle();
        }
#endif
    }
}

