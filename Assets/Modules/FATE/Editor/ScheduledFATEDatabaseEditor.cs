using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using com.playbux.map;
using System.Collections.Generic;

namespace com.playbux.FATE.editor
{

    [CustomEditor(typeof(ScheduledFATEDatabase))]
    public class ScheduledFATEDatabaseEditor : Editor
    {
        private bool debugFlag;
        private int hour;
        private int minute;
        private int mapIndex;
        private int fateIndex;
        private int positionIndex;
        private string searchFateKeyword;
        private DayOfWeek dayFilter;
        private DayOfWeek dayOfWeek;
        private FATEScheduleKey newKey;
        private FATEScheduleKey filterKey;
        private MapDatabase mapDatabase;
        private FATEDatabase fateDatabase;
        private ScheduledFATEDatabase database;
        private FATEPositionDatabase fatePositionDatabase;

        private string[] mapKeys;
        private string[] fateKeys;
        private string[] positionKeys;
        private string[] mapNameForSearch;
        private string[][] fateNameForSearch;
        private string[][] positionForSearch;
        private FATEScheduleData[] searchedFates;
        private List<int> minForEdit = new List<int>();
        private List<int> hourForEdit = new List<int>();
        private List<int> fateToEdit = new List<int>();
        private List<int> positionKeyForEdit = new List<int>();
        private List<DayOfWeek> dayForEdit = new List<DayOfWeek>();
        private List<int> mapToEdit = new List<int>();

        private void OnEnable()
        {
            dayOfWeek = DayOfWeek.Sunday;
            filterKey = new FATEScheduleKey();
            database = (ScheduledFATEDatabase)target;
            fateDatabase = AssetDatabase.LoadAssetAtPath<FATEDatabase>(AssetDatabase.GetAssetPath(database).Replace(database.name + ".asset", "FATEDatabase.asset"));
            mapDatabase = AssetDatabase.LoadAssetAtPath<MapDatabase>(AssetDatabase.GetAssetPath(database).Replace("FATE/" + database.name + ".asset", "Map/MapDatabase.asset"));
            fatePositionDatabase = AssetDatabase.LoadAssetAtPath<FATEPositionDatabase>(AssetDatabase.GetAssetPath(database).Replace(database.name + ".asset", "FATEPositionDatabase.asset"));

            mapKeys = new string[mapDatabase.Maps.Length];
            fateKeys = new string[fateDatabase.Data.Length];
            var positionKey = new FATEPositionKey();
            positionKey.map = mapDatabase.Maps[0].name;
            positionKey.fateId = fateDatabase.Data[0].id;
            var positions = fatePositionDatabase.Get(positionKey);

            if (positions != null)
            {
                positionKeys = new string[positions.Length];

                for (int i = 0; i < positions.Length; i++)
                {
                    positionKeys[i] = $"X:{positions[i].x} Y:{positions[i].y}";
                }
            }

            positionKeys ??= Array.Empty<string>();

            for (int i = 0; i < mapDatabase.Maps.Length; i++)
            {
                mapKeys[i] = mapDatabase.Maps[i].name;
            }

            for (int i = 0; i < fateDatabase.Data.Length; i++)
            {
                fateKeys[i] = fateDatabase.Data[i].name;
            }

            mapNameForSearch = mapDatabase.Maps.Select(m => m.name).ToArray();
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
            EditorGUI.BeginChangeCheck();
            mapIndex = EditorGUILayout.Popup(mapIndex, mapKeys);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("F.A.T.E To Add", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            fateIndex = EditorGUILayout.Popup(fateIndex, fateKeys);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                var positionKey = new FATEPositionKey();
                positionKey.map = mapDatabase.Maps[mapIndex].name;
                positionKey.fateId = fateDatabase.Data[fateIndex].id;
                var positions = fatePositionDatabase.Get(positionKey);

                if (positions != null)
                {
                    positionKeys = new string[positions.Length];

                    for (int i = 0; i < positions.Length; i++)
                    {
                        positionKeys[i] = $"X:{positions[i].x} Y:{positions[i].y}";
                    }
                }
            }

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Day of Week", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            dayOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup("", dayOfWeek);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Time of Day", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("Please enter 24 hours format", MessageType.Info);
            GUILayout.Label("Hours", EditorStyles.largeLabel);
            hour = EditorGUILayout.IntField("", hour);

            if (hour > 23)
                hour = 23;

            if (hour < 0)
                hour = 23;

            GUILayout.Label("Minutes", EditorStyles.largeLabel);
            minute = EditorGUILayout.IntField("", minute);

            if (minute > 60)
                minute = 0;

            if (minute < 0)
                minute = 60;

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Available Positions", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            positionIndex = EditorGUILayout.Popup(positionIndex, positionKeys);

            if (GUILayout.Button("Add F.A.T.E Schedule"))
            {
                var newTime = new DateTime(0);
                newTime = newTime.AddHours(hour);
                newTime = newTime.AddMinutes(minute);
                newKey.dayOfWeek = dayOfWeek;
                newKey.timeKey = newTime.Ticks;
                newKey.mapKey = mapDatabase.Maps[mapIndex].name;
                var newData = new FATEScheduleData();
                newData.data = fateDatabase.Data[fateIndex].Clone();
                var newPositionKey = new FATEPositionKey();
                newPositionKey.fateId = newData.data.id;
                newPositionKey.map = mapDatabase.Maps[mapIndex].name;
                var newPosition = fatePositionDatabase.Get(newPositionKey);

                if (newPosition == null)
                {
                    Debug.LogWarning("There is no available position for this F.A.T.E");
                    return;
                }

                newData.position = newPosition[positionIndex];
                database.Add(newKey, newData);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                newKey = new FATEScheduleKey();
                database.TrySave();
            }

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For Scheduled F.A.T.E", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Label("F.A.T.E Name/ID", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            searchFateKeyword = EditorGUILayout.TextField(searchFateKeyword);
            GUILayout.Label("Filter Day", EditorStyles.miniLabel);
            dayFilter = (DayOfWeek)EditorGUILayout.EnumPopup("", dayFilter);

            if (EditorGUI.EndChangeCheck())
            {
                filterKey.dayOfWeek = dayFilter;
                searchedFates = database.Get(filterKey, searchFateKeyword);

                if (searchedFates != null)
                {
                    var tempFates = new List<string[]>();
                    var tempPositionKeys = new List<string[]>();

                    for (int i = 0; i < searchedFates.Length; i++)
                    {
                        int searchMapIndex = 0;
                        var scheduleKeys = database.GetTime(searchedFates[i].data.id, dayFilter);

                        for (int j = 0; j < scheduleKeys.Length; j++)
                        {
                            for (int x = 0; x < mapDatabase.Maps.Length; x++)
                            {
                                if (scheduleKeys[j].mapKey != mapDatabase.Maps[x].name)
                                    continue;

                                searchMapIndex = x;
                            }
                        }

                        List<string> fates = new List<string>();

                        for (int j = 0; j < fateDatabase.Data.Length; j++)
                        {
                            if (fateDatabase.Data[j].id == searchedFates[i].data.id)
                                fateToEdit.Add(j);

                            fates.Add(fateDatabase.Data[j].name);
                        }

                        var positionKey = new FATEPositionKey();
                        positionKey.fateId = searchedFates[i].data.id;
                        positionKey.map = mapDatabase.Maps[searchMapIndex].name;
                        var position = fatePositionDatabase.Get(positionKey);
                        var positionList = new HashSet<string>();

                        for (int j = 0; j < position.Length; j++)
                        {
                            if (position[j] == searchedFates[i].position)
                                positionKeyForEdit.Add(j);

                            positionList.Add(position[j].ToString());
                        }

                        if (scheduleKeys.Length >= 0)
                        {
                            var time = new DateTime(scheduleKeys[i].timeKey);
                            hourForEdit.Add(time.Hour);
                            minForEdit.Add(time.Minute);
                            dayForEdit.Add(scheduleKeys[i].dayOfWeek);
                        }

                        mapToEdit.Add(searchMapIndex);
                        tempFates.Add(fates.ToArray());
                        tempPositionKeys.Add(positionList.ToArray());
                    }

                    fateNameForSearch = tempFates.ToArray();
                    positionForSearch = tempPositionKeys.ToArray();
                }
            }

            if (searchedFates == null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                return;
            }

            for (int i = 0; i < searchedFates.Length; i++)
            {
                var oldSchedKey = database.GetTime(searchedFates[i].data.id, dayFilter);
                uint oldId = searchedFates[i].data.id;

                EditorGUILayout.BeginVertical("Button");
                EditorGUI.BeginChangeCheck();
                GUILayout.Label($"Map");
                mapToEdit[i] = EditorGUILayout.Popup(mapToEdit[i], mapNameForSearch);
                GUILayout.Label($"F.A.T.E Schedule");
                dayForEdit[i] = (DayOfWeek)EditorGUILayout.EnumPopup(dayForEdit[i]);
                EditorGUILayout.BeginHorizontal();
                hourForEdit[i] = EditorGUILayout.DelayedIntField(hourForEdit[i]);
                minForEdit[i] = EditorGUILayout.DelayedIntField(minForEdit[i]);
                EditorGUILayout.EndHorizontal();
                GUILayout.Label($"F.A.T.E Name");
                fateToEdit[i] = EditorGUILayout.Popup(fateToEdit[i], fateNameForSearch[i]);
                GUILayout.Label($"F.A.T.E Position");
                positionKeyForEdit[i] = EditorGUILayout.Popup(positionKeyForEdit[i], positionForSearch[i]);
                EditorGUILayout.Space(2.5f);
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    var newSchedKey = new FATEScheduleKey();
                    var newPositionKey = new FATEPositionKey();
                    var newTime = new DateTime(0);
                    newTime = newTime.AddHours(hourForEdit[i]);
                    newTime = newTime.AddMinutes(minForEdit[i]);
                    newSchedKey.timeKey = newTime.Ticks;
                    newSchedKey.dayOfWeek = dayForEdit[i];
                    newSchedKey.mapKey = mapDatabase.Maps[mapToEdit[i]].name;
                    var newFateData = new FATEScheduleData();
                    newFateData.data = fateDatabase.Data[fateToEdit[i]].Clone();
                    newPositionKey.map = mapDatabase.Maps[mapToEdit[i]].name;
                    newPositionKey.fateId = newFateData.data.id;
                    var newFatePosition = fatePositionDatabase.Get(newPositionKey);

                    if (newFatePosition == null)
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        return;
                    }

                    newFateData.position = newFatePosition[positionKeyForEdit[i]];

                    if (!newSchedKey.Equals(oldSchedKey[i]))
                    {
                        database.Add(newSchedKey, newFateData);
                        database.Remove(oldSchedKey[i]);
                    }
                    else
                        database.Edit(newSchedKey, newFateData);

                    database.TrySave();
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
    }
}