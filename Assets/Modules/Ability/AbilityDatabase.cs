using System.Linq;
using UnityEngine;
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.ability
{
    [CreateAssetMenu(menuName = "Playbux/Ability/Create AbilityDatabase", fileName = "AbilityDatabase", order = 0)]
    public class AbilityDatabase : ScriptableObject
    {
        public uint[] Ids => data.Keys.ToArray();

        public string[] RecastStack => recastStack;

        public AbilityData[] Data => data.Values.ToArray();

        [SerializeField]
        private string[] recastStack;

        [SerializeField]
        private SerializedDictionary<uint, AbilityData> data = new SerializedDictionary<uint, AbilityData>();

        public bool HasKey(uint id)
        {
            return data.ContainsKey(id);
        }

        [CanBeNull]
        public AbilityData Get(uint id)
        {
            if (!HasKey(id))
                return null;

            return data[id];
        }

#if UNITY_EDITOR
        public void Add(uint id, AbilityData data)
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