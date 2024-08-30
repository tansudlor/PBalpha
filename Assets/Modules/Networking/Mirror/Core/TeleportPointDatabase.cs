using System.Linq;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.mirror.core
{
    [CreateAssetMenu(menuName = "Playbux/Teleportation/Create TeleportPointDatabase", fileName = "TeleportPointDatabase", order = 0)]
    public class TeleportPointDatabase : ScriptableObject
    {
        public string[] Keys => dictionary.Keys.ToArray();

        public TeleportPositionData[] Data => dictionary.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<string, TeleportPositionData> dictionary = new SerializedDictionary<string, TeleportPositionData>();

#if UNITY_EDITOR
        public void Add(string key, TeleportPositionData data)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, data);
                return;
            }

            dictionary[key] = data;
        }

        public void Remove(string key)
        {
            dictionary.Remove(key);
        }
#endif
    }
}