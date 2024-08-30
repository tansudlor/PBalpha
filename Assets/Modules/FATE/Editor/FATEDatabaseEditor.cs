using System.Collections.Generic;
using NanoidDotNet;
using UnityEditor;
using UnityEngine;

namespace com.playbux.FATE.editor
{
    [CustomEditor(typeof(FATEDatabase))]
    public class FATEDatabaseEditor : Editor
    {
        private bool isRandom;
        private bool debugFlag;
        private string keyword;
        private FATEData data;
        private FATEDatabase database;

        private void OnEnable()
        {
            data = new FATEData();
            database = (FATEDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");

            debugFlag = EditorGUILayout.ToggleLeft("Debug Mode", debugFlag);

            if (debugFlag)
            {
                base.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("F.A.T.E ID", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            data.id = (uint)database.Count + uint.Parse(Nanoid.Generate("0123456789", 4));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField((int)data.id);
            EditorGUI.EndDisabledGroup();
            GUILayout.Label("F.A.T.E Name", EditorStyles.largeLabel);
            data.name = EditorGUILayout.TextField(data.name);
            GUILayout.Label("F.A.T.E Description", EditorStyles.largeLabel);
            data.desc = EditorGUILayout.TextArea(data.desc);

            if (GUILayout.Button("Add F.A.T.E"))
            {
                var hasData = database.Get(data.id);

                if (hasData.HasValue)
                {
                    while (hasData.Value.id == data.id)
                        data.id = (uint)database.Count + uint.Parse(Nanoid.Generate("0123456789", 4));
                }

                database.Add(data);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                data = new FATEData();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For F.A.T.E", EditorStyles.largeLabel);
            keyword = EditorGUILayout.TextField(keyword);
            EditorGUILayout.Space(2.5f);
            EditorGUILayout.EndVertical();

            if (string.IsNullOrEmpty(keyword))
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginVertical("Box");
            for (int i = 0; i < database.Data.Length; i++)
            {
                if (!database.Data[i].name.ToLower().Contains(keyword.ToLower()) && !database.Data[i].id.ToString().Contains(keyword.ToLower()))
                    continue;

                EditorGUILayout.BeginVertical("Button");
                GUILayout.Label("Name", EditorStyles.miniLabel);
                database.Data[i].name = EditorGUILayout.TextField(database.Data[i].name);
                GUILayout.Label("Description", EditorStyles.miniLabel);
                database.Data[i].desc = EditorGUILayout.TextArea(database.Data[i].desc, EditorStyles.textArea);
                EditorGUILayout.Space(2.5f);

                if (GUILayout.Button($"Remove {database.Data[i].name}", EditorStyles.toolbarButton))
                {
                    database.Remove(database.Data[i]);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.Space(2.5f);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }
    }
}