using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.playbux.networking.mirror.client.building.editor
{
    [CustomEditor(typeof(CTEFlagDatabase))]
    public class CTEFlagDatabaseEditor : Editor
    {
        private bool debugMode;
        private CTEFlagDatabase database;
        private DefaultAsset targetFolder;

        private void OnEnable()
        {
            database = (CTEFlagDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");

            if (debugMode)
            {
                EditorGUILayout.BeginVertical("Button");
                debugMode = EditorGUILayout.ToggleLeft("Debug Mode", debugMode);
                EditorGUILayout.EndVertical();
                base.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                return;
            }

            debugMode = EditorGUILayout.ToggleLeft("Debug Mode", debugMode);
            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(targetFolder, typeof(DefaultAsset), false);

            if (GUILayout.Button("Add"))
            {
                var paths = Directory.GetFiles(AssetDatabase.GetAssetPath(targetFolder), "*.png");
                for (int i = 0; i < paths.Length; i++)
                {
                    Debug.Log(paths[i]);
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(paths[i]);
                    var key = texture.name.ToLower();
                    var firstChar = key[0].ToString().ToUpper();
                    key = key.Remove(0, 1);
                    key = firstChar + key;
                    database.Add(key, texture);
                }

                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.EndVertical();
        }
    }
}