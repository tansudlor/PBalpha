using System.Linq;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEditor;

namespace com.playbux.zone
{
    [CreateAssetMenu(menuName = "Playbux/Zone/Create ZoneDatabase", fileName = "ZoneDatabase", order = 0)]
    public class ZoneDatabase : ScriptableObject
    {
        public ZoneKey[] Keys => zones.Keys.ToArray();

        [SerializeField]
        private SerializedDictionary<ZoneKey, ZoneAsset> zones = new SerializedDictionary<ZoneKey, ZoneAsset>();

#if UNITY_EDITOR
        public string path => AssetDatabase.GetAssetPath(dataPath);

        [SerializeField]
        private DefaultAsset dataPath;

        public void Add(ZoneKey key, ZoneAsset asset)
        {
            if (!zones.ContainsKey(key))
            {
                zones.Add(key, asset);
                return;
            }

            zones[key] = asset;
        }

        public void Remove(ZoneKey key)
        {
            if (!zones.ContainsKey(key))
                return;

            zones.Remove(key);
        }
#endif

        public ZoneAsset? Get(ZoneKey key)
        {
            return !zones.ContainsKey(key) ? null : zones[key];
        }
    }
}