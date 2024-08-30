using UnityEngine;
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.mirror.client.building
{
    [CreateAssetMenu(menuName = "Playbux/Building/Create CTEFlagDatabase", fileName = "CTEFlagDatabase", order = 0)]
    public class CTEFlagDatabase : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<string, Texture2D> flagTextures = new SerializedDictionary<string, Texture2D>();

        [CanBeNull]
        public Texture2D Get(string key)
        {
            return flagTextures.TryGetValue(key, out var texture) ? texture : null;
        }

        public void Add(string key, Texture2D texture)
        {
            flagTextures[key] = texture;
        }
    }
}