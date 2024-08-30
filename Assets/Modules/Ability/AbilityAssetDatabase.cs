using System.Linq;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;

namespace com.playbux.ability
{
    [CreateAssetMenu(menuName = "Playbux/Ability/Create AbilityAssetDatabase", fileName = "AbilityAssetDatabase", order = 0)]
    public class AbilityAssetDatabase : ScriptableObject
    {
        public uint[] Ids => data.Keys.ToArray();

        public AbilityAssetData[] Data => data.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<uint, AbilityAssetData> data = new SerializedDictionary<uint, AbilityAssetData>();

        [CanBeNull]
        public AbilityAssetData Get(uint id)
        {
            if (!data.ContainsKey(id))
                return null;

            return data[id];
        }

#if UNITY_EDITOR
        public void Add(uint id, AbilityAssetData data)
        {
            this.data.Add(id, data);
        }

        public void Remove(uint id)
        {
            data.Remove(id);
        }
#endif
    }
}