#if SERVER
using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using com.playbux.networking.mirror.message;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using com.playbux.gameevent;
using System.ComponentModel;

namespace com.playbux.schedules
{
    //   BaseAPIEventSchedule<TDataStore,TReadableData,TEventWorker>
    public sealed class EventSchedule : BaseAPIEventSchedule<TickData<JToken>, JToken, QuizTimeEvent>
    {
        public EventSchedule(QuizTimeEvent manager, IServerNetworkMessageReceiver<MiniEventMessage> MessageReceiver) : base(manager, MessageReceiver)
        {
            RefreshTime = 30;
        }
        protected override void SetActiveAPI()
        {
            this.apiRoute = "/operations/quiz-schedules";
        }

        protected override void AddToSchedule(string json)
        {
            JObject schedulesData = JsonConvert.DeserializeObject<JObject>(json);
            for (int i = 0; i < schedulesData["data"].Children().Count(); i++)
            {
                DateTime sessionDate = schedulesData["data"][i]["session_date"].ToObject<DateTime>();
                TickData<JToken> data = new TickData<JToken>(sessionDate.Ticks, schedulesData["data"][i]);
                events.Add(data);
            }
        }

        protected override void AddTestMessage(NetworkConnectionToClient connection, List<TickData<JToken>> events, MiniEventMessage message)
        {
            var command = message.Command;
            if (command == "quiztimeeventstartnow")
            {

                string format = "{\"quiz\": \"{QUIZ_ID}\",\"session_date\": \"{QUIZ_TIME}\"}";
                format = format.Replace("{QUIZ_ID}", message.Message);


                DateTime time = DateTime.UtcNow;
                format = format.Replace("{QUIZ_TIME}", time.ToString("s"));

                JToken quizData = JToken.Parse(format);
                TickData<JToken> newEvent = new TickData<JToken>(time.Ticks, quizData);
                events.Add(newEvent);
                for (int i = 0; i < events.Count; i++)
                {
                    Debug.Log(i + " : " + JsonConvert.SerializeObject(events[i]));
                }


            }
        }
    }
}
#endif
