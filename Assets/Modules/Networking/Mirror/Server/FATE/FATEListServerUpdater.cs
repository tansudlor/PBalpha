using System;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.FATE;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class FATEListServerUpdater : IInitializable, ILateDisposable
    {
        private readonly IFATEScheduler scheduler;
        private readonly IServerMessageSender<FATEListUpdateMessage> updateSender;

        private DayOfWeek? currentDay;
        private DateTimeByte[] hourAndMinuteAsBytes;
        private FATEDataPack[] scheduledFateIds;

        public FATEListServerUpdater(IFATEScheduler scheduler, IServerMessageSender<FATEListUpdateMessage> updateSender)
        {
            this.scheduler = scheduler;
            this.updateSender = updateSender;
        }

        public void Initialize()
        {
            updateSender.Message += Send;
            updateSender.SendCondition += SendCondition;
            scheduler.Initialize();
        }
        public void LateDispose()
        {
            updateSender.Message -= Send;
            updateSender.SendCondition -= SendCondition;
            scheduler.Dispose();
        }

        private void GetCurrentDay()
        {
            if (currentDay.HasValue && currentDay.Value == DateTime.Now.DayOfWeek)
                return;
            
            currentDay ??= DateTime.Now.DayOfWeek;

            if (currentDay.HasValue && currentDay.Value != DateTime.Now.DayOfWeek)
                currentDay = DateTime.Now.DayOfWeek;
            
            var today = scheduler.Get();
            int count = 0;
            hourAndMinuteAsBytes = new DateTimeByte[today.Count];
            var fateList = new HashSet<uint>();
            var allTimeFateList = new List<FATEDataPack>();

            foreach (var pair in today)
            {
                var scheduledTimeAsDateTime = new DateTime(pair.Key);
                hourAndMinuteAsBytes[count] = new DateTimeByte(scheduledTimeAsDateTime);

                for (int i = 0; i < pair.Value.Length; i++)
                    fateList.Add(pair.Value[i].data.id);

                allTimeFateList.Add(new FATEDataPack(fateList.ToArray()));

                count++;
            }

            scheduledFateIds = allTimeFateList.ToArray();
            
#if DEVELOPMENT
            string fates = "Today F.A.T.Es schedule are\n";

            foreach (var pair in today)
            {
                var dateTime = new DateTime(0);
                dateTime = dateTime.AddTicks(pair.Key);
                fates += "[" + dateTime.ToString("HH:mm") + "]\n";

                for (int i = 0; i < pair.Value.Length; i++)
                {
                    fates += pair.Value[i].data.name;
                    fates += "\n";
                }
            }
            
            Debug.Log(fates);
#endif
        }

        private bool SendCondition()
        {
            GetCurrentDay();

            if (hourAndMinuteAsBytes == null)
                return false;
            
            if (scheduledFateIds == null)
                return false;

            return scheduledFateIds.Length > 0;
        }

        private FATEListUpdateMessage Send()
        {
            return new FATEListUpdateMessage(currentDay.Value, hourAndMinuteAsBytes, scheduledFateIds);
        }
    }
}