using UnityEditor;
using UnityEngine;
using com.playbux.map;
using System.Collections.Generic;

namespace com.playbux.FATE.editor
{
    [CustomEditor(typeof(FATEPositionDatabase))]
    public class FATEPositionDatabaseEditor : Editor
    {
        private bool debugFlag;
        private int mapIndex;
        private int fateIndex;
        private int searchMapIndex;
        private int searchFateIndex;

        private MapDatabase mapDatabase;
        private FATEDatabase fateDatabase;
        private FATEPositionDatabase database;
        private List<Vector2> positions = new List<Vector2>();
        private List<Vector2> searchedPositions = new List<Vector2>();
        private List<Vector2> cachedSearchedPositions = new List<Vector2>();

        private string[] mapKeys;
        private string[] fateKeys;

        private void OnEnable()
        {
            database = (FATEPositionDatabase)target;
            fateDatabase = AssetDatabase.LoadAssetAtPath<FATEDatabase>(AssetDatabase.GetAssetPath(database).Replace(database.name + ".asset", "FATEDatabase.asset"));
            mapDatabase = AssetDatabase.LoadAssetAtPath<MapDatabase>(AssetDatabase.GetAssetPath(database).Replace("FATE/" + database.name + ".asset", "Map/MapDatabase.asset"));

            mapKeys = new string[mapDatabase.Maps.Length];
            fateKeys = new string[fateDatabase.Data.Length];

            for (int i = 0; i < mapDatabase.Maps.Length; i++)
            {
                mapKeys[i] = mapDatabase.Maps[i].name;
            }

            for (int i = 0; i < fateDatabase.Data.Length; i++)
            {
                fateKeys[i] = fateDatabase.Data[i].name;
            }

            searchedPositions.Clear();
            var searchKey = new FATEPositionKey();
            searchKey.map = mapDatabase.Maps[searchMapIndex].name;
            searchKey.fateId = fateDatabase.Data[searchFateIndex].id;
            var data = database.Get(searchKey);

            if (data == null)
                return;

            for (int i = 0; i < data.Length; i++)
                searchedPositions.Add(data[i]);
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

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Target Map", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            mapIndex = EditorGUILayout.Popup(mapIndex, mapKeys);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Target F.A.T.E", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            fateIndex = EditorGUILayout.Popup(fateIndex, fateKeys);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Position", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Add position"))
                positions.Add(Vector2.zero);

            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] = EditorGUILayout.Vector2Field($"#{i + 1}", positions[i]);
            }

            EditorGUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(positions.Count <= 0);
            if (GUILayout.Button("Save"))
            {
                var key = new FATEPositionKey();
                key.map = mapDatabase.Maps[mapIndex].name;
                key.fateId = fateDatabase.Data[fateIndex].id;
                database.Add(key, positions.ToArray());
                positions.Clear();
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For F.A.T.E Position", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Label("Map Filter", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            searchMapIndex = EditorGUILayout.Popup(searchMapIndex, mapKeys);
            GUILayout.Label("F.A.T.E Filter", EditorStyles.miniLabel);
            searchFateIndex = EditorGUILayout.Popup(searchFateIndex, fateKeys);

            if (EditorGUI.EndChangeCheck())
            {
                searchedPositions.Clear();
                var searchKey = new FATEPositionKey();
                searchKey.map = mapDatabase.Maps[searchMapIndex].name;
                searchKey.fateId = fateDatabase.Data[searchFateIndex].id;
                var data = database.Get(searchKey);

                if (data == null)
                    return;

                for (int i = 0; i < data.Length; i++)
                {
                    searchedPositions.Add(data[i]);
                    cachedSearchedPositions.Add(data[i]);
                }
            }

            for (int i = 0; i < searchedPositions.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical("Button");
                GUILayout.Label($"Position#{i + 1}");
                float x = EditorGUILayout.DelayedFloatField("X: ", searchedPositions[i].x);
                float y = EditorGUILayout.DelayedFloatField("Y: ", searchedPositions[i].y);
                EditorGUILayout.Space(2.5f);
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    var searchKey = new FATEPositionKey();
                    searchKey.map = mapDatabase.Maps[searchMapIndex].name;
                    searchKey.fateId = fateDatabase.Data[searchFateIndex].id;
                    database.Add(searchKey, new Vector2(x, y));
                    database.Remove(searchKey, cachedSearchedPositions[i]);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    searchedPositions.Clear();
                    cachedSearchedPositions.Clear();

                    var data = database.Get(searchKey);

                    if (data == null)
                        return;

                    for (int j = 0; j < data.Length; j++)
                    {
                        searchedPositions.Add(data[j]);
                        cachedSearchedPositions.Add(data[j]);
                    }

                    return;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }
    }
}