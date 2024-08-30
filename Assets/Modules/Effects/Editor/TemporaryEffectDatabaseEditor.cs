using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using NanoidDotNet;
using System.Collections.Generic;

namespace com.playbux.effects.editor
{
    [CustomEditor(typeof(TemporaryEffectDatabase))]
    public class TemporaryEffectDatabaseEditor : Editor
    {
        private bool debugMode;
        private uint newId;
        private string searchKeyword;
        private int idIndex;
        private string[] availableIds;
        private TemporaryEffectData newEffect;
        private TemporaryEffectDatabase database;
        private StatusStackDatabase statusStackDatabase;

        private TemporaryEffectData[] data;

        private void OnEnable()
        {
            newEffect = new TemporaryEffectData();
            database = (TemporaryEffectDatabase)target;
            statusStackDatabase = AssetDatabase.LoadAssetAtPath<StatusStackDatabase>(AssetDatabase.
                GetAssetPath(database).
                Replace(database.name + ".asset", "StatusStackDatabase.asset"));
            newEffect.potencies = Array.Empty<EffectPotency>();
            data = database.Data.ToArray();

            availableIds = statusStackDatabase.Ids;
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

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Create New Effect", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            newId = (uint)EditorGUILayout.IntField("", (int)newId);
            if (GUILayout.Button("Random ID"))
            {
                uint tempId = uint.Parse(Nanoid.Generate("0123456789", 5));

                while (database.Ids.Contains(tempId))
                    tempId = uint.Parse(Nanoid.Generate("0123456789", 5));

                newId = tempId;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Effect Name", EditorStyles.miniLabel);
            newEffect.name = EditorGUILayout.TextField(newEffect.name);
            GUILayout.Label("Effect Description", EditorStyles.miniLabel);
            newEffect.desc = EditorGUILayout.TextArea(newEffect.desc);
            GUILayout.Label("Effect Duration", EditorStyles.miniLabel);
            newEffect.duration = EditorGUILayout.FloatField(newEffect.duration);
            GUILayout.Label("Effect Stack Id", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            idIndex = EditorGUILayout.Popup(idIndex, availableIds);

            if (EditorGUI.EndChangeCheck())
                newEffect.stackId = availableIds[idIndex];

            var potencies = new HashSet<EffectPotency>();
            if (newEffect.potencies.Length <= 0)
                potencies = newEffect.potencies.ToHashSet();

            for (int i = 0; i < newEffect.potencies.Length; i++)
            {
                GUILayout.Label("Effect Potency Type", EditorStyles.miniLabel);
                newEffect.potencies[i].type = (EffectPotencyType)EditorGUILayout.EnumPopup(newEffect.potencies[i].type);
                GUILayout.Label("Effect Potency", EditorStyles.miniLabel);
                newEffect.potencies[i].potency = EditorGUILayout.FloatField(newEffect.potencies[i].potency);

                if (GUILayout.Button("Remove this potency type"))
                {
                    potencies.Remove(newEffect.potencies[i]);
                    EditorGUILayout.EndVertical();
                    return;
                }
            }

            if (GUILayout.Button("Add more potency"))
            {
                potencies.Add(new EffectPotency(EffectPotencyType.Flat, 1));
                newEffect.potencies = potencies.ToArray();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(1f);

            if (GUILayout.Button("Add Status Effect"))
            {
                uint tempId = newId;

                while (database.Ids.Contains(tempId))
                    tempId = uint.Parse(Nanoid.Generate("0123456789", 5));

                database.Add(tempId, newEffect);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                newEffect = new TemporaryEffectData();
                newEffect.potencies = Array.Empty<EffectPotency>();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For Effect", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            searchKeyword = EditorGUILayout.TextField(searchKeyword, EditorStyles.toolbarSearchField);

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(searchKeyword))
                    break;

                if (!data[i].name.ToLower().Contains(searchKeyword) || !data[i].desc.ToLower().Contains(searchKeyword))
                    continue;

                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.Space(1.5f);
                GUILayout.Label("Effect ID", EditorStyles.miniLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(database.Ids[i].ToString());
                EditorGUI.EndDisabledGroup();
                GUILayout.Label("Effect Name", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                data[i].name = EditorGUILayout.TextField(data[i].name);
                GUILayout.Label("Effect Description", EditorStyles.miniLabel);
                data[i].desc = EditorGUILayout.TextArea(data[i].desc);
                GUILayout.Label("Effect Duration", EditorStyles.miniLabel);
                data[i].duration = EditorGUILayout.DelayedFloatField(data[i].duration);
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    database.Edit(database.Ids[i], data[i].Clone());
                    data = database.Data.ToArray();
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}