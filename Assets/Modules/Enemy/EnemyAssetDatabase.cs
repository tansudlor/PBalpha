using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.enemy
{
    [CreateAssetMenu(menuName = "Playbux/Enemy/Create EnemyAssetDatabase", fileName = "EnemyAssetDatabase", order = 0)]
    public class EnemyAssetDatabase : ScriptableObject
    {
        public uint[] AssetIds => assetByAssetId.Keys.ToArray();
        
        [SerializeField]
        private SerializedDictionary<uint, GameObject> assets = new SerializedDictionary<uint, GameObject>();

        [SerializeField]
        private SerializedDictionary<uint, GameObject> assetByAssetId = new SerializedDictionary<uint, GameObject>();

        [CanBeNull]
        public GameObject Get(uint id)
        {
            if (!assets.ContainsKey(id) && !assetByAssetId.ContainsKey(id))
                return null;

            if (!assets.ContainsKey(id) && assetByAssetId.TryGetValue(id, out var value))
                return value;

            return assets[id];
        }

        public bool Contains(uint assetId) => assetByAssetId.ContainsKey(assetId);

#if UNITY_EDITOR
        public void AssignAssetId()
        {
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