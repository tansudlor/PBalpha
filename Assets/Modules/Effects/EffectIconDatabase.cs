using System.Linq;
using UnityEngine;
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.effects
{

    [CreateAssetMenu(menuName = "Playbux/Status Effect/Create EffectIconDatabase", fileName = "EffectIconDatabase", order = 0)]
    public class EffectIconDatabase : ScriptableObject
    {
        public uint[] Ids => icons.Keys.ToArray();

        public Texture2D[] Icons => icons.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<uint, Texture2D> icons = new SerializedDictionary<uint, Texture2D>();

        [CanBeNull]
        public Texture2D Get(uint id)
        {
            return !icons.ContainsKey(id) ? null : icons[id];

        }

#if UNITY_EDITOR
        public void Edit(uint id, Texture2D texture2D)
        {
            if (!icons.ContainsKey(id))
                return;

            icons[id] = texture2D;
        }
        public void Add(uint id, Texture2D texture2D)
        {
            icons.Add(id, texture2D);
        }

        public void Remove(uint id) => icons.Remove(id);
#endif
    }
}