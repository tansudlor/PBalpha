using UnityEditor;
using UnityEngine;

namespace com.playbux.map.editor
{

    [CustomEditor(typeof(MapColliderDatabase))]
    public class MapColliderDatabaseEditor : Editor
    {
        private bool debugFlag;
        private string search;
        private string newName;
        private GameObject targetPrefab;
        private MapColliderDatabase database;

        private void OnEnable()
        {
            search = "";
            database = (MapColliderDatabase)target;
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

            GUILayout.Label("Name", EditorStyles.miniLabel);
            newName = EditorGUILayout.TextField(newName);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newName));
            targetPrefab = (GameObject)EditorGUILayout.ObjectField(targetPrefab, typeof(GameObject), false);

            EditorGUI.BeginDisabledGroup(targetPrefab == null);

            if (GUILayout.Button("Create"))
            {
                database.Create(newName, targetPrefab);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Search", EditorStyles.miniLabel);
            search = EditorGUILayout.TextField(search);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            foreach (var key in database.Keys)
            {
                if (search.Length < 3)
                    break;

                if (!key.ToLower().Contains(search.ToLower()))
                    continue;

                EditorGUILayout.BeginVertical("Button");
                GUILayout.Label($"Name: {key}", EditorStyles.largeLabel);
                EditorGUILayout.EndVertical();

                foreach (var mapPair in database.Get(key))
                {
                    EditorGUILayout.BeginVertical("Button");
                    var position = mapPair.Key;
                    var col = mapPair.Value;
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Label("Position", EditorStyles.miniLabel);
                    position = EditorGUILayout.Vector2Field("", position);
                    GUILayout.Label("Collider", EditorStyles.miniLabel);
                    col = (Collider2D)EditorGUILayout.ObjectField(mapPair.Value, typeof(Collider2D), false);

                    EditorGUILayout.Space(1.25f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        database.Edit(key, position, col);
                        EditorUtility.SetDirty(database);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndVertical();
                        break;
                    }

                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Remove"))
                {
                    database.Remove(key);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}