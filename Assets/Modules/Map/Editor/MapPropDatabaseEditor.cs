using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.map.editor
{
    [CustomEditor(typeof(MapPropDatabase))]
    public class MapPropDatabaseEditor : Editor
    {
        private bool debugFlag;
        private int propIndex;
        private int mapIdIndex;
        private string searchForKeyword;
        private MapDatabase mapDatabase;
        private MapPropDatabase database;
        private PropDatabase propDatabase;
        private GameObject targetGameObject;

        private string[] mapIds;

        private void OnEnable()
        {
            searchForKeyword = "";
            database = (MapPropDatabase)target;
            string path = AssetDatabase.GetAssetPath(database);
            string mapPath = path.Replace("MapPropDatabase.asset", "MapDatabase.asset");
            string propPath = path.Replace("MapPropDatabase.asset", "PropDatabase.asset");
            mapDatabase = AssetDatabase.LoadAssetAtPath<MapDatabase>(mapPath);
            propDatabase = AssetDatabase.LoadAssetAtPath<PropDatabase>(propPath);

            mapIds = mapDatabase.Maps.Select(m => m.name).ToArray();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            debugFlag = EditorGUILayout.ToggleLeft("Debug?", debugFlag);

            if (debugFlag)
            {
                base.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Add New Prop", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            GUILayout.Label("Target Map", EditorStyles.miniLabel);
            mapIdIndex = EditorGUILayout.Popup(mapIdIndex, mapIds);

            targetGameObject = (GameObject)EditorGUILayout.ObjectField(targetGameObject, typeof(GameObject), true);

            EditorGUI.BeginDisabledGroup(targetGameObject == null);
            if (GUILayout.Button("Import Prop Data"))
            {
                for (int i = 0; i < targetGameObject.transform.childCount; i++)
                {
                    var child = targetGameObject.transform.GetChild(i).gameObject;
                    bool isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(child);

                    if (!isPrefab)
                    {
                        Debug.LogWarning($"GameObject {child.name} is not a prefab");
                        continue;
                    }

                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var propData = new PropDataWrapper();
                    bool isPrefabHasNegativeX = prefab.transform.lossyScale.x < 0;
                    bool isGameObjectHasNegativeX = child.transform.lossyScale.x < 0;
                    propData.name = prefab.name;
                    propData.scale = child.transform.lossyScale;
                    propData.position = child.transform.position;
                    bool flip = false;

                    if (isPrefabHasNegativeX && isGameObjectHasNegativeX)
                        flip = false;

                    if (isPrefabHasNegativeX && !isGameObjectHasNegativeX)
                        flip = true;

                    if (!isPrefabHasNegativeX && isGameObjectHasNegativeX)
                        flip = true;

                    if (!isPrefabHasNegativeX && !isGameObjectHasNegativeX)
                        flip = false;

                    propData.flip = flip;
                    database.Add(mapIds[mapIdIndex], propData);
                }

                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();

            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search Box", EditorStyles.largeLabel);
            searchForKeyword = EditorGUILayout.TextField(searchForKeyword, EditorStyles.toolbarSearchField);
            EditorGUILayout.EndVertical();

            for (int i = 0; i < database.Keys.Length; i++)
            {
                if (searchForKeyword.Length < 3)
                    break;

                if (!database.Keys[i].ToLower().Contains(searchForKeyword.ToLower()))
                    continue;

                if (GUILayout.Button("Try Generate"))
                {
                    var parent = new GameObject();
                    parent.transform.position = Vector3.zero;
                    parent.name = $"{database.Keys[i]}_props";

                    for (int j = 0; j < database.Data[i].data.Length; j++)
                    {
                        var data = database.Data[i].data[j];
                        var prop = propDatabase.Get(data.name);

                        if (!prop.HasValue)
                            continue;

                        var instance = Instantiate(prop.Value.propObject, data.position, Quaternion.identity);

                        for (int k = 0; k < prop.Value.propCollider.Length; k++)
                        {
                            var rotation = prop.Value.propCollider[k].transform.rotation;

                            if (data.flip)
                                rotation.z *= -1;

                            var collider = Instantiate(prop.Value.propCollider[k], data.position, rotation);
                            collider.transform.localScale = data.scale;
                            collider.transform.SetParent(parent.transform);
                        }

                        instance.transform.localScale = data.scale;
                        instance.transform.SetParent(parent.transform);
                    }
                }
            }


            EditorGUILayout.EndVertical();
        }
    }
}