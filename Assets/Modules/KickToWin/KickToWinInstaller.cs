using com.playbux.networking.mirror.message;
using com.playbux.ui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.kicktowin
{
    [CreateAssetMenu(menuName = "Playbux/KickToWin/KickToWinInstaller", fileName = "KickToWinInstaller")]
    public class KickToWinInstaller : ScriptableObjectInstaller<KickToWinInstaller>
    {
        [SerializeField]
        private KickToWinMapData kickToWinMapData;

        [SerializeField]
        private CircleScript circleKickToWin;

        [SerializeField]
        private KickToWinTimmer kickToWinTimmer;

        [SerializeField]
        private KickToWinBallAnim kickToWinBallAnim;

        [SerializeField]
        private KickCoinController kickCoinController;

        [SerializeField]
        private GameObject KickToWinPrefab;


        private UICanvas canvas;
        [Inject]
        void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }


        public override void InstallBindings()
        {

            Container.Bind<KickToWin>().AsSingle().NonLazy();
            Container.Bind<CircleScript>().FromInstance(circleKickToWin).AsSingle();


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

            Container.Bind<KickToWinMapData>().FromInstance(kickToWinMapData).AsSingle().NonLazy();

            Container.Bind(
                   typeof(IInitializable),
                   typeof(ILateDisposable),
                   typeof(IServerNetworkMessageReceiver<KickToWinMessage>))
               .To<ServerNetworkMessageReceiver<KickToWinMessage>>().AsSingle();

            Container.Bind(
                  typeof(IInitializable),
                  typeof(ILateDisposable),
                  typeof(IServerNetworkMessageReceiver<ResyncMessage>))
              .To<ServerNetworkMessageReceiver<ResyncMessage>>().AsSingle();

        }
#endif

#if !SERVER
        private void BindClientSide()
        {
            Container.Bind(
                  typeof(IInitializable),
                  typeof(ILateDisposable),
                  typeof(INetworkMessageReceiver<KickToWinMessage>))
              .To<ClientMessageReceiver<KickToWinMessage>>().AsSingle();

            Container.Bind<KickToWinTimmer>().FromInstance(kickToWinTimmer).AsSingle();

            Container.Bind<KickToWinBallAnim>().FromInstance(kickToWinBallAnim).AsSingle();

            Container.Bind<KickToWinUI>()
               .FromComponentInNewPrefab(KickToWinPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle().NonLazy();

            Container.Bind<KickCoinController>().FromInstance(kickCoinController).AsSingle();

            Container.Bind(
                  typeof(IInitializable),
                  typeof(ILateDisposable),
                  typeof(INetworkMessageReceiver<ResyncMessage>))
              .To<ClientMessageReceiver<ResyncMessage>>().AsSingle();

        }
#endif
    }

}

