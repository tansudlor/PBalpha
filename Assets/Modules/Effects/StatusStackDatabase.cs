using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
namespace com.playbux.effects
{
    [CreateAssetMenu(menuName = "Playbux/Status Effect/Create StatusStackDatabase", fileName = "StatusStackDatabase", order = 0)]
    public class StatusStackDatabase : ScriptableObject
    {
        public string[] Ids => stackData.Keys.ToArray();

        [SerializeField]
        private SerializedDictionary<string, uint> stackData = new SerializedDictionary<string, uint>();

        [CanBeNull]
        public uint? Get(string id)
        {
            return !stackData.ContainsKey(id) ? null : stackData[id];

        }

#if UNITY_EDITOR
        public void Edit(string id, uint count)
        {
            if (!stackData.ContainsKey(id))
                return;

            stackData[id] = count;
        }
        public void Add(string id, uint count)
        {
            stackData.Add(id, count);
        }

        public void Remove(string id) => stackData.Remove(id);
#endif
    }
}