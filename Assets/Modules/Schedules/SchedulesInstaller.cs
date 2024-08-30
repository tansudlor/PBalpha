
using com.playbux.networking.mirror.message;
using UnityEngine;
using com.playbux.gameevent;
using Zenject;

namespace com.playbux.schedules
{
    [CreateAssetMenu(menuName = "Playbux/Schedules/SchedulesInstaller", fileName = "SchedulesInstaller")]
    public class SchedulesInstaller : ScriptableObjectInstaller<SchedulesInstaller>
    {

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

            Container.Bind<EventSchedule>().AsSingle().NonLazy();


          

        }
#endif

#if !SERVER
        private void BindClientSide()
        {

        }
#endif
    }
}

