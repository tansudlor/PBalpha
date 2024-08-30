using System;
using UnityEditor;
using UnityEngine;

namespace com.playbux.networking.mirror.core.editor
{
    [CustomEditor(typeof(TeleportPointDatabase))]
    public class TeleportPointDatabaseEditor : Editor
    {
        private bool debugMode;
        private string newKey;
        private Transform newAreaPosition;
        private TeleportableArea newPrefab;
        private Transform newTargetPosition;
        private TeleportPointDatabase database;

        private void OnEnable()
        {
            database = (TeleportPointDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            debugMode = EditorGUILayout.ToggleLeft("Debug Mode", debugMode);

            if (debugMode)
            {
                base.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("New Teleport Area", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Key", EditorStyles.miniLabel);
            newKey = EditorGUILayout.TextField(newKey);
            GUILayout.Label("Area Position", EditorStyles.miniLabel);
            newAreaPosition = (Transform)EditorGUILayout.ObjectField(newAreaPosition, typeof(Transform), true);
            GUILayout.Label("Target Position", EditorStyles.miniLabel);
            newTargetPosition = (Transform)EditorGUILayout.ObjectField(newTargetPosition, typeof(Transform), true);
            GUILayout.Label("Area GameObject", EditorStyles.miniLabel);
            newPrefab = (TeleportableArea)EditorGUILayout.ObjectField(newPrefab, typeof(TeleportableArea), true);

            bool isKeyEmpty = string.IsNullOrEmpty(newKey);
            bool isNewAreaPositionNull = newAreaPosition == null;
            bool isNewTargetPositionNull = newTargetPosition == null;
            bool isNewPrefabNull = newPrefab == null;

            EditorGUI.BeginDisabledGroup(isKeyEmpty || isNewAreaPositionNull || isNewTargetPositionNull || isNewPrefabNull);
            EditorGUILayout.Space(0.5f);
            if (GUILayout.Button("Add"))
            {
                var newData = new TeleportPositionData();
                newData.areaPrefab = newPrefab;
                newData.areaPosition = newAreaPosition.position;
                newData.targetPosition = newTargetPosition.position;
                database.Add(newKey, newData);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(0.5f);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }
    }
}