using System;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.FATE;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client.FATE
{
    public class FATEClientListController : IInitializable, ILateDisposable, ILateTickable
    {
        private readonly FATEDatabase fateDatabase;
        private readonly INetworkMessageReceiver<FATEListUpdateMessage> updateReceiver;

        private DayOfWeek? today;
        private Dictionary<long, FATEData[]> currentDay = new Dictionary<long, FATEData[]>();
        
        public FATEClientListController(FATEDatabase fateDatabase, INetworkMessageReceiver<FATEListUpdateMessage> updateReceiver)
        {
            this.fateDatabase = fateDatabase;
            this.updateReceiver = updateReceiver;
        }
        
        public void Initialize()
        {
            updateReceiver.OnEventCalled += OnUpdateReceived;
        }

        public void LateDispose()
        {
            updateReceiver.OnEventCalled -= OnUpdateReceived;
        }
        
        public void LateTick()
        {
            if (!today.HasValue)
                return;
                
            if (today.Value == DateTime.Now.DayOfWeek)
                return;

            today = DateTime.Now.DayOfWeek;
            updateReceiver.OnEventCalled += OnUpdateReceived;
        }

        private void OnUpdateReceived(FATEListUpdateMessage message)
        {
            currentDay.Clear();
            
            if (message.keys == null)
                return;
            
            if (message.fateIds == null)
                return;

            for (int i = 0; i < message.keys.Length; i++)
            {
                var now = DateTime.Now;
                var fateList = new HashSet<FATEData>();
                DateTime schedTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
                schedTime = schedTime.AddHours(message.keys[i].hour);
                schedTime = schedTime.AddMinutes(message.keys[i].minute);

                for (int j = 0; j < message.fateIds[i].ids.Length; j++)
                {
                    var fateData = fateDatabase.Get(message.fateIds[i].ids[j]);
                    
                    if (!fateData.HasValue)
                        continue;
                    
                    fateList.Add(fateData.Value);
                }
                
                currentDay.Add(schedTime.Ticks, fateList.ToArray());
            }

            today = message.dayOfWeek;
            updateReceiver.OnEventCalled -= OnUpdateReceived;
            
#if DEVELOPMENT
            string fates = "Today F.A.T.Es schedule are\n";

            foreach (var pair in currentDay)
            {
                var dateTime = new DateTime(0);
                dateTime = dateTime.AddTicks(pair.Key);
                fates += "[" + dateTime.ToString("HH:mm") + "]\n";

                for (int i = 0; i < pair.Value.Length; i++)
                {
                    fates += pair.Value[i].name;
                    fates += "\n";
                }
            }
            
            Debug.Log(fates);
#endif
        }
    }
}
