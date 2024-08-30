using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.map
{
    [CreateAssetMenu(menuName = "Playbux/Map/Create MapBuildingDatabase", fileName = "MapBuildingDatabase", order = 0)]
    public class MapBuildingDatabase : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<string, MapBuildingDataCollection> buildings = new SerializedDictionary<string, MapBuildingDataCollection>();

#if UNITY_EDITOR
        public void Add(string key, MapBuildingData prefab)
        {
            var tempList = buildings.TryGetValue(key, out var building) ? building.data.ToList() : new List<MapBuildingData>();
            tempList.Add(prefab);
            var collection = new MapBuildingDataCollection();
            collection.data = tempList.ToArray();
            buildings[key] = collection;
        }

        public void Remove(string key)
        {
            buildings.Remove(key);
        }
#endif

        [CanBeNull]
        public MapBuildingData[] Get(string key)
        {
            return !buildings.ContainsKey(key) ? null : buildings[key].data;
        }
    }
}