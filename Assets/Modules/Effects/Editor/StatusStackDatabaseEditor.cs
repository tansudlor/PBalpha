using System.Linq;
using UnityEditor;
using UnityEngine;
using NanoidDotNet;

namespace com.playbux.effects.editor
{
    [CustomEditor(typeof(StatusStackDatabase))]
    public class StatusStackDatabaseEditor : Editor
    {
        private const int MAX_CHARACTER = 18;
        private const string RANDOM_CHARACTERS = "_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private bool debugMode;
        private string newId;
        private string searchKeyword;
        private uint count;

        private StatusStackDatabase database;

        private void OnEnable()
        {
            database = (StatusStackDatabase)target;
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

            EditorGUILayout.BeginVertical("Button");
            debugMode = EditorGUILayout.ToggleLeft("Debug Mode", debugMode);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Create New Status Stack", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            newId = EditorGUILayout.TextField(newId);
            if (GUILayout.Button("Random ID"))
            {
                string tempId = Nanoid.Generate(RANDOM_CHARACTERS, MAX_CHARACTER);

                while (database.Ids.Contains(tempId))
                    tempId = Nanoid.Generate(RANDOM_CHARACTERS, MAX_CHARACTER);

                newId = tempId;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Stack Count", EditorStyles.miniLabel);
            count = (uint)EditorGUILayout.IntSlider((int)count, 1, 99);

            if (GUILayout.Button("Add Status Stack"))
            {
                string tempId = newId;

                while (database.Ids.Contains(tempId))
                    tempId = Nanoid.Generate(RANDOM_CHARACTERS, MAX_CHARACTER);

                database.Add(tempId, count);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();

                newId = "";
                count = 1;

                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndVertical();
        }
    }
}