#if SERVER
using System;
using System.Collections.Generic;
using com.playbux.api;
using com.playbux.gameevent;
using com.playbux.networking.mirror.message;
using com.playbux.schedulessetting;
using Cysharp.Threading.Tasks;
using Mirror;
using Newtonsoft.Json.Linq;
using UnityEngine;



namespace com.playbux.schedules
{

    public abstract class BaseAPIEventSchedule<TDataStore, TReadableData, TEventManager> where TDataStore : ITickData, IValueReader<TReadableData> where TEventManager : BaseGameEvent<TReadableData>
    {
        protected TEventManager manager;
        protected List<TDataStore> events = new List<TDataStore>();
        protected string apiRoute = "";
        private int refreshTime = 30;
        public int RefreshTime { get => refreshTime; set => refreshTime = value; }
        IServerNetworkMessageReceiver<MiniEventMessage> miniEventMessageReceiver;

        public BaseAPIEventSchedule(TEventManager manager, IServerNetworkMessageReceiver<MiniEventMessage> miniEventMessageReceiver)
        {
            Debug.Log("hashCode Worker:" + manager.GetHashCode());
            SetActiveAPI();
            APITimer().Forget();
            this.manager = manager;
            this.miniEventMessageReceiver = miniEventMessageReceiver;
            this.miniEventMessageReceiver.OnEventCalled += OnMiniEventMessageReceiver;
        }

        private void OnMiniEventMessageReceiver(NetworkConnectionToClient connection, MiniEventMessage message, int channel)
        {
#if DEVELOPMENT
            Debug.Log("Server ESM received message: " + message);
            Debug.Log("Server ESM Find NetworkIdentity");
            Debug.Log("Require ESM *************************" + connection);
#endif
            AddTestMessage(connection,events, message);
            
        }

        async UniTask RunSchedule()
        {



#if DEV_QUIZ_EVENT
            await UniTask.Delay(120000);
            Debug.Log(Path.Combine(Application.dataPath , "Modules/Schedules/DevSchudel.json"));
            string json = File.ReadAllText(Path.Combine(Application.dataPath, "Modules/Schedules/DevSchudel.txt"));
            TEventWorker workerTest = GetWorker(json);
            workerTest.RunEvent();
            return;
#endif

            for (int i = events.Count - 1; i >= 0; i--)
            {
                if (events[i].Tick < DateTime.UtcNow.Ticks)
                {
                    events.RemoveAt(i);
                }
            }

            while (events.Count > 0)
            {

                //loop all event
                for (int i = 0; i < events.Count; i++)
                {
                    // check if date time in event list < current time
                    if (events[i].Tick <= DateTime.UtcNow.Ticks)
                    {
                        
                       //Debug.Log("DateTime.UtcNow.Ticks  " + DateTime.UtcNow.Ticks);
                       //Debug.Log("events["+i +"].Tick" + events[i].Tick);
                       //Debug.Log("DateTime.UtcNow.Ticks - events[i].Tick : " + APIServerConnector.TicksToSecond(DateTime.UtcNow.Ticks - events[i].Tick));
                        
                        if(events[i].Tick < DateTime.UtcNow.Ticks)
                        {
                            //  run event is trigger
                            Debug.Log("run Event");
                            manager.SetJobDiscription(events[i].Read());
                            manager.RunEvent();
                            //markremove
                            events[i].Tick = -events[i].Tick;

                        }

                    }
                }
                

                //loop remove obsolete 
                for (int i = events.Count - 1; i >= 0; i--)
                {
                    if (events[i].Tick < 0)
                    {
                        events.RemoveAt(i);
                        
                    }
                }
                //Debug.Log("[QuizTimeEvent]" + events.Count);
                //Debug.Log("eventSum : " + events.Count);
                await UniTask.Delay(1000 * 10);

            }
        }

       


        async UniTask APITimer()
        {
            while (true)
            {
                Debug.Log("[EventScheduler]: APITimer() check for download time-table " + events.Count);
                if (events.Count <= 0)
                {
                    //call load job
                    string json = await APIServerConnector.SchedulesAPI(apiRoute, ScheduleSetting.DebugSchedule);

                    //add job
                    AddToSchedule(json);
                    //run job
                    RunSchedule().Forget();
                    Debug.Log("[EventScheduler] : APITimer() new Quiz caller " + events.Count);
                }
                
                await UniTask.Delay(1000 * 60 * RefreshTime);

            }

        }
        protected abstract void SetActiveAPI();
        protected abstract void AddToSchedule(string json);
        protected virtual void AddTestMessage(NetworkConnectionToClient connection, List<TDataStore> events, MiniEventMessage message)
        {
            Debug.Log("No Polymorph for test");
        }

    }

}
#endif