using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace com.playbux.effects.editor
{

    [CustomEditor(typeof(EffectIconDatabase))]
    public class EffectIconDatabaseEditor : Editor
    {
        private bool debugMode;
        private string searchKeyword;
        private int indexId;
        private Texture2D newIcon;
        private EffectIconDatabase database;
        private TemporaryEffectDatabase temporaryEffectDatabase;
        private PermanentEffectDatabase permanentEffectDatabase;

        private int[] searchedIdForEdit;
        private string[] availableIds;
        private Texture2D[] icon;
        private TemporaryEffectData[] temporaryEffectDatas;
        private PermanentEffectData[] permanentEffectDatas;

        private void OnEnable()
        {
            database = (EffectIconDatabase)target;
            icon = database.Icons.ToArray();
            temporaryEffectDatabase = AssetDatabase.LoadAssetAtPath<TemporaryEffectDatabase>(AssetDatabase.
                GetAssetPath(database).
                Replace(database.name + ".asset", "TemporaryEffectDatabase.asset"));

            permanentEffectDatabase = AssetDatabase.LoadAssetAtPath<PermanentEffectDatabase>(AssetDatabase.
                GetAssetPath(database).
                Replace(database.name + ".asset", "PermanentEffectDatabase.asset"));

            icon = database.Icons.ToArray();
            temporaryEffectDatas = temporaryEffectDatabase.Data.ToArray();
            permanentEffectDatas = permanentEffectDatabase.Data.ToArray();
            searchedIdForEdit = new int[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];
            availableIds = new string[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];

            for (int x = 0; x < temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length; x++)
            {
                if (x >= temporaryEffectDatabase.Ids.Length)
                {
                    int index = x - temporaryEffectDatabase.Ids.Length;
                    availableIds[x] = permanentEffectDatabase.Ids[index].ToString();
                    continue;
                }

                availableIds[x] = temporaryEffectDatabase.Ids[x].ToString();
            }
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
            GUILayout.Label("Link New Effect Icon", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            GUILayout.Label("Effect Id", EditorStyles.miniLabel);
            indexId = EditorGUILayout.Popup(indexId, availableIds);
            GUILayout.Label("Effect Icon", EditorStyles.miniLabel);
            newIcon = (Texture2D)EditorGUILayout.ObjectField(newIcon, typeof(Texture2D), false);

            EditorGUILayout.Space(1f);

            if (GUILayout.Button("Add Status Effect"))
            {
                database.Add(temporaryEffectDatabase.Ids[indexId], newIcon);
                icon = database.Icons.ToArray();
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For Effect Icons", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            searchKeyword = EditorGUILayout.TextField(searchKeyword, EditorStyles.toolbarSearchField);

            for (int i = 0; i < icon.Length; i++)
            {
                if (string.IsNullOrEmpty(searchKeyword))
                    break;

                var tempData = temporaryEffectDatabase.Get(database.Ids[i]);
                var permData = permanentEffectDatabase.Get(database.Ids[i]);

                string dataName = tempData == null ? permData == null ? "" : permData.name : tempData.name;
                string dataDesc = tempData == null ? permData == null ? "" : permData.desc : tempData.desc;

                if (!dataName.ToLower().Contains(searchKeyword) || !dataDesc.ToLower().Contains(searchKeyword))
                    continue;

                uint oldId = database.Ids[i];

                for (int j = 0; j < availableIds.Length; j++)
                {
                    if (availableIds[j] != database.Ids[i].ToString())
                        continue;

                    searchedIdForEdit[i] = j;
                }

                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label($"{dataName}", EditorStyles.largeLabel);
                EditorGUILayout.EndVertical();
                GUILayout.Label("ID", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                searchedIdForEdit[i] = EditorGUILayout.Popup(searchedIdForEdit[i], availableIds);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorGUILayout.EndVertical();


                    if (searchedIdForEdit[i] < temporaryEffectDatabase.Ids.Length)
                    {
                        database.Add(temporaryEffectDatabase.Ids[searchedIdForEdit[i]], icon[i]);
                        database.Remove(oldId);
                    }
                    else
                    {
                        int calculatedIndex = searchedIdForEdit[i] - temporaryEffectDatabase.Ids.Length;
                        database.Add(permanentEffectDatabase.Ids[calculatedIndex], icon[i]);
                        database.Remove(oldId);
                    }

                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();

                    icon = database.Icons.ToArray();
                    temporaryEffectDatas = temporaryEffectDatabase.Data.ToArray();
                    permanentEffectDatas = permanentEffectDatabase.Data.ToArray();
                    searchedIdForEdit = new int[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];
                    availableIds = new string[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];

                    for (int x = 0; x < temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length; x++)
                    {
                        if (x >= temporaryEffectDatabase.Ids.Length)
                        {
                            int index = x - temporaryEffectDatabase.Ids.Length;
                            availableIds[x] = permanentEffectDatabase.Ids[index].ToString();
                            continue;
                        }

                        availableIds[x] = temporaryEffectDatabase.Ids[x].ToString();
                    }

                    return;
                }

                GUILayout.Label("Icon", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                icon[i] = (Texture2D)EditorGUILayout.ObjectField(icon[i], typeof(Texture2D), false);
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    if (searchedIdForEdit[i] < temporaryEffectDatabase.Ids.Length)
                    {
                        database.Edit(temporaryEffectDatabase.Ids[i], icon[i]);
                    }
                    else
                    {
                        int calculatedIndex = searchedIdForEdit[i] - temporaryEffectDatabase.Ids.Length;
                        database.Edit(permanentEffectDatabase.Ids[calculatedIndex], icon[i]);
                    }

                    database.Edit(temporaryEffectDatabase.Ids[i], icon[i]);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    icon = database.Icons.ToArray();
                    temporaryEffectDatas = temporaryEffectDatabase.Data.ToArray();
                    permanentEffectDatas = permanentEffectDatabase.Data.ToArray();
                    searchedIdForEdit = new int[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];
                    availableIds = new string[temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length];

                    for (int x = 0; x < temporaryEffectDatabase.Ids.Length + permanentEffectDatabase.Ids.Length; x++)
                    {
                        if (x >= temporaryEffectDatabase.Ids.Length)
                        {
                            int index = x - temporaryEffectDatabase.Ids.Length;
                            availableIds[x] = $"[{permanentEffectDatabase.Ids[index]}] {permanentEffectDatabase.Data[index].name}";
                            continue;
                        }

                        availableIds[x] = $"[{temporaryEffectDatabase.Ids[x]}] {temporaryEffectDatabase.Data[x].name}";
                    }
                    return;
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}