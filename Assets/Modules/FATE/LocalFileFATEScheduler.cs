using System;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.io;
using com.playbux.map;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.FATE
{
    public struct FATENotificationKey
    {
        public uint id;
        public long startTime;
        public long notifyTime;
    }

    public class LocalFileFATEScheduler : IFATEScheduler, ILateTickable
    {
        public event Action<FATEData> OnFATEStarted;
        public event Action<DateTime, FATEData> OnFATENotified;
        
        private const long REPEAT_NOTIFY_TIME = 3000000000; //NOTE: 5 mins of ticks

        private readonly IMapController controller;
        private readonly IAsyncFileReader<ScheduledFATEData> dataReader;

        private List<long> fateToRemove = new List<long>();
        private List<FATENotificationKey> fateNotificationToRemove = new List<FATENotificationKey>();
        private Dictionary<long, FATEScheduleData[]> currentDay = new Dictionary<long, FATEScheduleData[]>();
        private Dictionary<FATEScheduleKey, FATEScheduleData[]> schedule = new Dictionary<FATEScheduleKey, FATEScheduleData[]>();
        private Dictionary<FATENotificationKey, FATEScheduleData> notifyingFate = new Dictionary<FATENotificationKey, FATEScheduleData>();
        private Dictionary<DayOfWeek, Dictionary<long, FATEScheduleData[]>> routineSchedule = new Dictionary<DayOfWeek, Dictionary<long, FATEScheduleData[]>>();

        public LocalFileFATEScheduler(IMapController controller, IAsyncFileReader<ScheduledFATEData> dataReader)
        {
            this.controller = controller;
            this.dataReader = dataReader;
            controller.OnCreated += TryReadSchedule;
        }

        public void LateTick()
        {
            if (currentDay.Count <= 0)
                return;

            DateTime now = DateTime.Now;

            foreach (var pair in currentDay)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    ScheduleNotification(now.Ticks, pair.Key, pair.Value[i]);

                    if (pair.Key > now.Ticks)
                        continue;

                    fateToRemove.Add(pair.Key);
                    OnFATEStarted?.Invoke(pair.Value[i].data);
#if DEVELOPMENT
                    Debug.Log($"FATE {pair.Value[i].data.name} has been started");
#endif
                }
            }

            foreach (var pair in notifyingFate)
            {
                if (fateNotificationToRemove.Contains(pair.Key))
                    continue;

                if (pair.Key.notifyTime > now.Ticks)
                    continue;

                fateNotificationToRemove.Add(pair.Key);
                var dateTime = new DateTime(pair.Key.startTime - pair.Key.notifyTime);
                OnFATENotified?.Invoke(dateTime, pair.Value.data);
#if DEVELOPMENT
                Debug.Log($"FATE {pair.Value.data.name} has been notified {new DateTime(pair.Key.notifyTime):HH:mm}");
#endif
            }

            for (int i = 0; i < fateNotificationToRemove.Count; i++)
            {
                if (fateToRemove.Count <= 0)
                    break;
                
                if (fateNotificationToRemove[i].notifyTime > now.Ticks)
                    continue;

                notifyingFate.Remove(fateNotificationToRemove[i]);
                fateNotificationToRemove.RemoveAt(i);
                i--;
            }

            if (fateToRemove.Count <= 0)
                return;

            int count = fateToRemove.Count;
            for (int i = 0; i < count; i++)
            {
                if (fateToRemove[i] > now.Ticks)
                    continue;

                fateToRemove.RemoveAt(i);
                currentDay.Remove(fateToRemove[i]);
            }
        }

        public void Initialize()
        {

        }
        public void Dispose()
        {
            controller.OnCreated -= TryReadSchedule;
        }

        public Dictionary<long, FATEScheduleData[]> Get()
        {
            return currentDay;
        }

        public Dictionary<long, FATEScheduleData[]> Get(DayOfWeek day)
        {
            return routineSchedule[day];
        }

        public FATEScheduleData[] Get(long hourAndMinute)
        {
            return Get(DateTime.Now.DayOfWeek, hourAndMinute);
        }

        public FATEScheduleData[] Get(DayOfWeek day, long hourAndMinute)
        {
            return routineSchedule[day][hourAndMinute];
        }

        private void ScheduleNotification(long now, long schedTime, FATEScheduleData fateData)
        {
            if (schedTime - REPEAT_NOTIFY_TIME < now)
                return;
            
            long fiveMins = schedTime - REPEAT_NOTIFY_TIME;
            var fiveMinsKey = new FATENotificationKey();
            fiveMinsKey.id = fateData.data.id;
            fiveMinsKey.startTime = schedTime;
            fiveMinsKey.notifyTime = fiveMins;

            notifyingFate.TryAdd(fiveMinsKey, fateData);
            
            if (schedTime - REPEAT_NOTIFY_TIME * 2 < now)
                return;
            
            long tenMins = schedTime - REPEAT_NOTIFY_TIME * 2;
            var tenMinsKey = new FATENotificationKey();
            tenMinsKey.id = fateData.data.id;
            tenMinsKey.startTime = schedTime;
            tenMinsKey.notifyTime = tenMins;

            notifyingFate.TryAdd(tenMinsKey, fateData);
            
            if (schedTime - REPEAT_NOTIFY_TIME * 3 < now)
                return;
            
            long fifteenMins = schedTime - REPEAT_NOTIFY_TIME * 3;
            var fifteenMinsKey = new FATENotificationKey();
            fifteenMinsKey.id = fateData.data.id;
            fifteenMinsKey.startTime = schedTime;
            fifteenMinsKey.notifyTime = fifteenMins;

            notifyingFate.TryAdd(fifteenMinsKey, fateData);
        }

        private void TryReadSchedule(string mapName)
        {
            ReadSchedule(mapName).Forget();
        }

        private async UniTaskVoid ReadSchedule(string mapName)
        {
            var data = await dataReader.Read();
            schedule = new Dictionary<FATEScheduleKey, FATEScheduleData[]>();
            var mapSchedule = new Dictionary<string, List<FATEScheduleData>>();
            
            for (int i = 0; i < data.keys.Length; i++)
            {
                schedule.Add(data.keys[i], data.data[i]);
                
                if (!mapSchedule.ContainsKey(data.keys[i].mapKey))
                    mapSchedule.Add(data.keys[i].mapKey, new List<FATEScheduleData>());

                for (int j = 0; j < data.data[i].Length; j++)
                {
                    mapSchedule[data.keys[i].mapKey].Add(data.data[i][j]);
                }
            }

            if (!mapSchedule.ContainsKey(mapName))
                return;

            foreach (var pair in schedule)
            {
                routineSchedule.TryAdd(pair.Key.dayOfWeek, new Dictionary<long, FATEScheduleData[]>());
                var fateList = new HashSet<FATEScheduleData>();

                foreach (var value in pair.Value)
                {
                    fateList.Add(value);
                }
                
                routineSchedule[pair.Key.dayOfWeek].Add(pair.Key.timeKey, fateList.ToArray());
            }

            DayOfWeek today = DateTime.Now.DayOfWeek;

            if (!routineSchedule.ContainsKey(today))
                return;

            foreach (var pair in routineSchedule[today])
                ScheduleDailyFATE(pair.Key, pair.Value);
        }

        private void ScheduleDailyFATE(long recordedTicks, FATEScheduleData[] data)
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
            scheduledTime = scheduledTime.AddTicks(recordedTicks);
            if (now > scheduledTime)
                return;

            currentDay.TryAdd(scheduledTime.Ticks, data);

#if DEVELOPMENT
            for (int i = 0; i < data.Length; i++)
                Debug.Log($"FATE {data[i].data.name} has been added");
#endif
        }
    }
}