using System;
using System.Linq;
using UnityEngine;
using com.playbux.io;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.FATE
{
    [Serializable]
    public struct FATEScheduleKey : IEquatable<FATEScheduleKey>
    {
        public string mapKey;
        public long timeKey;
        public DayOfWeek dayOfWeek;

        public bool Equals(FATEScheduleKey other)
        {
            return mapKey == other.mapKey && timeKey == other.timeKey && dayOfWeek == other.dayOfWeek;
        }

        public override bool Equals(object obj)
        {
            return obj is FATEScheduleKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(mapKey, timeKey, (int)dayOfWeek);
        }
    }

    [CreateAssetMenu(menuName = "Playbux/FATE/Create ScheduledFATEDatabase", fileName = "ScheduledFATEDatabase", order = 0)]
    public class ScheduledFATEDatabase : ScriptableObject
    {
        public FileInfo FileInfo => fileInfo;

        [SerializeField]
        private FileInfo fileInfo;

        [SerializeField]
        [SerializedDictionary("Key", "F.A.T.E")]
        private SerializedDictionary<FATEScheduleKey, FATEScheduleData[]> scheduledFate = new SerializedDictionary<FATEScheduleKey, FATEScheduleData[]>();


#if UNITY_EDITOR
        public void Add(FATEScheduleKey key, FATEScheduleData data)
        {
            if (!scheduledFate.ContainsKey(key))
                scheduledFate.TryAdd(key, Array.Empty<FATEScheduleData>());

            var dataHashSet = scheduledFate[key] == null ? new HashSet<FATEScheduleData>() : scheduledFate[key].ToHashSet();
            dataHashSet.Add(data);

            scheduledFate[key] = dataHashSet.ToArray();
        }

        public void Edit(FATEScheduleKey key, FATEScheduleData data)
        {
            if (!scheduledFate.ContainsKey(key))
                return;

            var dataHashSet = scheduledFate[key] == null ? new HashSet<FATEScheduleData>() : scheduledFate[key].ToHashSet();
            dataHashSet.Add(data);

            scheduledFate[key] = dataHashSet.ToArray();
        }

        public void Remove(FATEScheduleKey key)
        {
            scheduledFate.Remove(key);
        }

        public void Remove(uint fateId)
        {
            FATEScheduleKey? key = null;

            foreach (var pair in scheduledFate)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    if (pair.Value[i].data.id != fateId)
                        continue;

                    key = pair.Key;
                    break;
                }
            }

            if (!key.HasValue)
                return;

            scheduledFate.Remove(key.Value);
        }

        public FATEScheduleKey[] GetTime(uint fateId, DayOfWeek dayOfWeek)
        {
            var keys = new HashSet<FATEScheduleKey>();

            foreach (var pair in scheduledFate)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    if (pair.Key.dayOfWeek != dayOfWeek)
                        continue;
                    
                    if (pair.Value[i].data.id != fateId)
                        continue;

                    keys.Add(pair.Key);
                    break;
                }
            }

            return keys.ToArray();
        }

        public FATEScheduleData[]? GetData(FATEScheduleKey key)
        {
            if (!scheduledFate.ContainsKey(key))
                return null;

            if (scheduledFate[key] == null)
                return null;

            if (scheduledFate[key].Length <= 0)
                return null;

            return scheduledFate[key];
        }

        public FATEScheduleData[]? Get(FATEScheduleKey key, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return null;

            var searchedFate = new List<FATEScheduleData>();

            foreach (var pair in scheduledFate)
            {
                if (pair.Key.dayOfWeek != key.dayOfWeek)
                    continue;

                for (int i = 0; i < pair.Value.Length; i++)
                {
                    if (!pair.Value[i].data.name.ToLower().Contains(keyword.ToLower()) && !pair.Value[i].data.id.ToString().Contains(keyword.ToLower()))
                        continue;

                    searchedFate.Add(pair.Value[i]);
                }
            }

            return searchedFate.Count <= 0 ? null : searchedFate.ToArray();
        }

        public void EditFileInfo(string name, string path, string extension)
        {
            fileInfo = new FileInfo(name, path, extension);
        }

        public void TrySave()
        {
            Save().Forget();
        }

        private async UniTaskVoid Save()
        {
            var data = new ScheduledFATEData();
            data.keys = scheduledFate.Keys.ToArray();
            data.data = scheduledFate.Values.ToArray();
            IAsyncFileWriter<ScheduledFATEData> writer = new JSONFileWriter<ScheduledFATEData>(fileInfo);
            await writer.Write(data);
        }
#endif
    }

    [Serializable]
    public class ScheduledFATEData
    {
        public FATEScheduleKey[] keys;
        public FATEScheduleData[][] data;
    }
}