using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace com.playbux.FATE
{
    public class RandomFATEDatabase : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<FATERarity, FATEData> data = new SerializedDictionary<FATERarity, FATEData>();

        public void Add(FATERarity rarity, FATEData data)
        {

        }

        public void Remove(FATEData data)
        {

        }
    }
}