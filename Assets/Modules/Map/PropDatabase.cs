using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.playbux.map
{
    [CreateAssetMenu(menuName = "Playbux/Map/Create PropDatabase", fileName = "PropDatabase")]
    public class PropDatabase : ScriptableObject
    {
        public string[] Keys => props.Keys.ToArray();
        public PropData[] Data => props.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<string, PropData> props = new SerializedDictionary<string, PropData>();

#if UNITY_EDITOR
        public DefaultAsset dataFolder;

        public void Create(GameObject gameObject)
        {
            var propData = new PropData();
            GameObject prefab = gameObject;

            string path = AssetDatabase.GetAssetPath(dataFolder);

            bool isPrefab = !PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) && !PrefabUtility.IsPartOfAnyPrefab(gameObject);

            var colliders = prefab.GetComponentsInChildren<Collider2D>();
            var instanceCols = new List<Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                var colPrefab = PrefabUtility.SaveAsPrefabAsset(colliders[i].gameObject, $"{path}/{gameObject.name}_collider_{i}.prefab");
                instanceCols.Add(colPrefab.GetComponent<Collider2D>());
                colliders[i].transform.SetParent(null);
            }

            if (isPrefab)
                prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, $"{path}/{gameObject.name}.prefab");

            propData.propObject = prefab;

            colliders = instanceCols.ToArray();
            propData.propCollider = colliders;

            if (props.ContainsKey(gameObject.name))
            {
                props[gameObject.name] = propData;
                return;
            }

            props.Add(gameObject.name, propData);
        }

        public void Edit(string propKey, GameObject gameObject, Collider2D[] colliders)
        {
            var propData = new PropData();
            GameObject prefab = gameObject;

            string path = AssetDatabase.GetAssetPath(dataFolder);

            if (!PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) && !PrefabUtility.IsPartOfAnyPrefab(gameObject))
                prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, $"{path}/{gameObject.name}.prefab");

            propData.propObject = prefab;

            propData.propCollider = colliders;

            if (props.ContainsKey(propKey))
            {
                props[propKey] = propData;
                return;
            }

            props.Add(propKey, propData);
        }

        public void Remove(string propKey)
        {
            props.Remove(propKey);
        }
#endif
        
        public PropData? Get(string name)
        {
            if (!props.ContainsKey(name))
                return null;

            return props[name];
        }
    }
}