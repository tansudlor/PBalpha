using com.playbux.networking.mirror.message;
using UnityEngine;
using Zenject;

namespace com.playbux.gameevent
{
    [CreateAssetMenu(menuName = "Playbux/GameEvent/GameEventInstaller", fileName = "GameEventInstaller")]
    public class GameEventInstaller : ScriptableObjectInstaller<GameEventInstaller>
    {
        [SerializeField]
        private GameObject gameEventServerPrefab;

        public override void InstallBindings()
        {



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

            Container.Bind<QuizTimeEvent>().FromComponentInNewPrefab(gameEventServerPrefab).AsTransient();

            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<MiniEventMessage>))
                .To<ServerNetworkMessageReceiver<MiniEventMessage>>().AsSingle();



        }
#endif

#if !SERVER
        private void BindClientSide()
        {
        Container.Bind<GameEventClient>().AsSingle().NonLazy();

            

            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<MiniEventMessage>))
                .To<ClientMessageReceiver<MiniEventMessage>>().AsSingle();
        }
#endif
    }

}

