using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.fakeplayer
{
    [CreateAssetMenu(menuName = "Playbux/FakePlayer/Create FakePlayerAssetDatabase", fileName = "FakePlayerAssetDatabase", order = 0)]
    public class FakePlayerAssetDatabase : ScriptableObject
    {
        [Serializable]
        public class RandomBotWrapper
        {
            public uint id;
            public GameObject asset;
        }

        public RandomBotWrapper RandomAssetById => randomAssetById;

        public GameObject[] Assets => assetByAssetId.Values.ToArray();

        public uint[] AssetIds => assetByAssetId.Keys.ToArray();

        [SerializeField]
        private string[] randomNames;

        [SerializeField]
        private RandomBotWrapper randomAssetById;

        [SerializeField]
        private SerializedDictionary<uint, GameObject> assets = new SerializedDictionary<uint, GameObject>();

        [SerializeField]
        private SerializedDictionary<uint, GameObject> assetByAssetId = new SerializedDictionary<uint, GameObject>();

        [CanBeNull]
        public GameObject Get(uint id)
        {
            return !assetByAssetId.ContainsKey(id) ? randomAssetById.asset : assetByAssetId[id];
        }
        
        public int GetIndex(uint id)
        {
            int index = -1;

            if (!assetByAssetId.ContainsKey(id))
                return index;

            var keys = assetByAssetId.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != id)
                    continue;

                return i;
            }
            
            return index;
        }

        public bool Contains(uint assetId) => assetByAssetId.ContainsKey(assetId);

#if UNITY_EDITOR
        public void AssignAssetId()
        {
            if (randomAssetById?.asset != null)
            {
                uint assetId = 0;
                string path = AssetDatabase.GetAssetPath(randomAssetById?.asset);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Guid guid = new Guid(AssetDatabase.AssetPathToGUID(path));
                    assetId = (uint)guid.GetHashCode();
                }

                randomAssetById.id = assetId;
            }

            foreach (var pair in assets)
            {
                uint assetId = 0;
                string path = AssetDatabase.GetAssetPath(pair.Value);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Guid guid = new Guid(AssetDatabase.AssetPathToGUID(path));
                    assetId = (uint)guid.GetHashCode();
                }

                assetByAssetId.Add(assetId, pair.Value);
            }
        }
#endif
    }
}