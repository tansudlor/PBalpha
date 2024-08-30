using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
namespace com.playbux.effects
{
    [CreateAssetMenu(menuName = "Playbux/Status Effect/Create PermanentEffectDatabase", fileName = "PermanentEffectDatabase", order = 0)]
    public class PermanentEffectDatabase : ScriptableObject
    {
        public uint[] Ids => data.Keys.ToArray();
        public PermanentEffectData[] Data => data.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<uint, PermanentEffectData> data = new SerializedDictionary<uint, PermanentEffectData>();

        [CanBeNull]
        public PermanentEffectData Get(uint id)
        {
            if (!data.ContainsKey(id))
                return null;

            return data[id];
        }

#if UNITY_EDITOR
        public void Add(uint id, PermanentEffectData data) => this.data.Add(id, data);
        public void Edit(uint id, PermanentEffectData data)
        {
            this.data[id] = data;
        }
#endif
    }
}