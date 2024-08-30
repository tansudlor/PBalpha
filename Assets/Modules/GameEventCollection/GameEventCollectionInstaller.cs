using com.playbux.networking.mirror.message;
using UnityEngine;
using Zenject;

namespace com.playbux.gameeventcollection
{
    [CreateAssetMenu(menuName = "Playbux/GameEvent/EventCollectionInstaller", fileName = "EventCollectionInstaller")]
    public class GameEventCollectionInstaller : ScriptableObjectInstaller<GameEventCollectionInstaller>
    {

        [SerializeField]
        private PositionCollection positionCollectionData;
        public override void InstallBindings()
        {



#if !SERVER
            BindClientSide();

#endif

#if SERVER
            BindServerSide();
#endif

            Container.Bind<PositionCollection>().FromInstance(positionCollectionData).AsSingle().NonLazy();

        }
#if SERVER

        private void BindServerSide()
        {


            Container.Bind<GameEventCollection>().AsSingle().NonLazy(); 
            Container.BindInterfacesAndSelfTo<EventScoreCollector>().AsSingle().NonLazy();


        }
#endif

#if !SERVER
        private void BindClientSide()
        {
           

        }
#endif
    }

}


