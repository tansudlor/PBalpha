using System.Linq;
using UnityEngine;
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.effects
{

    [CreateAssetMenu(menuName = "Playbux/Status Effect/Create TemporaryEffectDatabase", fileName = "TemporaryEffectDatabase", order = 0)]
    public class TemporaryEffectDatabase : ScriptableObject
    {
        public uint[] Ids => data.Keys.ToArray();
        public TemporaryEffectData[] Data => data.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<uint, TemporaryEffectData> data = new SerializedDictionary<uint, TemporaryEffectData>();

        [CanBeNull]
        public TemporaryEffectData Get(uint id)
        {
            if (!data.ContainsKey(id))
                return null;

            return data[id];
        }

#if UNITY_EDITOR
        public void Add(uint id, TemporaryEffectData data) => this.data.Add(id, data);
        public void Edit(uint id, TemporaryEffectData data)
        {
            this.data[id] = data;
        }
#endif
    }

}