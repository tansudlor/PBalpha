using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.map
{

    [CreateAssetMenu(menuName = "Playbux/Map/Create ColliderDatabase", fileName = "ColliderDatabase")]
    public class MapColliderDatabase : ScriptableObject
    {
        public string[] Keys => mapColliders.Keys.ToArray();

        [SerializeField]
        private SerializedDictionary<string, SerializedDictionary<Vector2, Collider2D>> mapColliders = new SerializedDictionary<string, SerializedDictionary<Vector2, Collider2D>>();

#if UNITY_EDITOR
        public void Create(string name, GameObject prefab)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            string folderPath = prefabPath.Replace($"{prefab.name}.prefab", "");
            var colliders = prefab.GetComponentsInChildren<CompositeCollider2D>(true);
            var dict = new SerializedDictionary<Vector2, Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                var col = Instantiate(colliders[i]);
                var colPrefab = PrefabUtility
                    .SaveAsPrefabAsset(col.gameObject, $"{folderPath}{name}_collider#{i}_{colliders[i].name}.prefab")?
                    .GetComponent<Collider2D>();

                if (Equals(colliders[i], null))
                    continue;

                dict.TryAdd(colliders[i].transform.position, colPrefab);
                Debug.Log($"Added {colliders[i].name}");
                DestroyImmediate(col.gameObject);
            }

            mapColliders.TryAdd(name, dict);
        }

        public void Edit(string mapKey, Vector2 position, Collider2D value)
        {
            bool hasKey = mapColliders[mapKey].ContainsKey(position);

            if (!hasKey)
            {
                mapColliders[mapKey].TryAdd(position, value);
                return;
            }

            mapColliders[mapKey][position] = value;
        }

        public void Remove(string name)
        {
            mapColliders.Remove(name);
        }
#endif

        public bool ContainsKey(string name)
        {
            return mapColliders.ContainsKey(name);
        }

        public Dictionary<Vector2, Collider2D> Get(string name)
        {
            if (!mapColliders.ContainsKey(name))
                return null;

            var dict = new Dictionary<Vector2, Collider2D>();

            foreach (var pair in mapColliders[name])
                dict.Add(pair.Key, pair.Value);

            return dict;
        }
    }

}