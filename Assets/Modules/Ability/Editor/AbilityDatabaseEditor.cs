using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NanoidDotNet;
using com.playbux.effects;
using System.Collections.Generic;

namespace com.playbux.ability.editor
{
    [CustomEditor(typeof(AbilityDatabase))]
    public class AbilityDatabaseEditor : Editor
    {
        private bool debugMode;
        private uint newId;
        private string searchKeyword;
        private int recastStackIndex;
        private AbilityData newAbility;
        private AbilityDatabase database;
        private TemporaryEffectDatabase temporaryEffectDatabase;
        private PermanentEffectDatabase permanentEffectDatabase;

        private int[] newEffectIndex;
        private int[] effectIndexForEdit;
        private string[] availableEffects;
        private uint[][] availableEffectIdsForEdit;
        private string[][] availableEffectNamesForEdit;

        private void OnEnable()
        {
            newAbility = new AbilityData();
            database = (AbilityDatabase)target;
            newAbility.effects = Array.Empty<uint>();
            newAbility.potencies = Array.Empty<AbilityPotency>();
            temporaryEffectDatabase = AssetDatabase.LoadAssetAtPath<TemporaryEffectDatabase>(AssetDatabase.GetAssetPath(database).Replace("Ability/" + database.name + ".asset", "Effects/TemporaryEffectDatabase.asset"));
            permanentEffectDatabase = AssetDatabase.LoadAssetAtPath<PermanentEffectDatabase>(AssetDatabase.GetAssetPath(database).Replace("Ability/" + database.name + ".asset", "Effects/PermanentEffectDatabase.asset"));

            newEffectIndex = new int[temporaryEffectDatabase.Data.Length + permanentEffectDatabase.Data.Length];
            availableEffects = new string[temporaryEffectDatabase.Data.Length + permanentEffectDatabase.Data.Length];

            for (int i = 0; i < availableEffects.Length; i++)
            {
                if (i >= temporaryEffectDatabase.Data.Length)
                {
                    int index = i - temporaryEffectDatabase.Data.Length;
                    availableEffects[i] = permanentEffectDatabase.Data[index].name;
                    continue;
                }

                availableEffects[i] = temporaryEffectDatabase.Data[i].name;
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
            GUILayout.Label("Create New Ability", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            GUILayout.Label("Ability ID", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            newId = (uint)EditorGUILayout.IntField("", (int)newId);
            if (GUILayout.Button("Random ID"))
            {
                uint tempId = newId;

                while (database.Ids.Contains(tempId))
                    tempId = uint.Parse(Nanoid.Generate("0123456789", 5));

                newId = tempId;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Ability Name", EditorStyles.miniLabel);
            newAbility.name = EditorGUILayout.TextField(newAbility.name);
            GUILayout.Label("Ability Description", EditorStyles.miniLabel);
            newAbility.desc = EditorGUILayout.TextArea(newAbility.desc);
            GUILayout.Label("Ability Type", EditorStyles.miniLabel);
            newAbility.abilityType = (AbilityType)EditorGUILayout.EnumPopup(newAbility.abilityType);

            if (newAbility.abilityType == AbilityType.Circuitcast)
            {
                GUILayout.Label("Ability Cast Time", EditorStyles.miniLabel);
                newAbility.castTime = EditorGUILayout.FloatField("", newAbility.castTime);
            }

            if (newAbility.abilityType != AbilityType.Ability)
            {
                GUILayout.Label("Ability Recast Time", EditorStyles.miniLabel);
                newAbility.recastTime ??= new RecastData();
                newAbility.recastTime.recastStack = database.RecastStack[0];
                newAbility.recastTime.time = EditorGUILayout.FloatField("", newAbility.recastTime.time);
                EditorGUI.BeginChangeCheck();
                recastStackIndex = EditorGUILayout.Popup(recastStackIndex, database.RecastStack);
                if (EditorGUI.EndChangeCheck())
                    newAbility.recastTime.recastStack = database.RecastStack[recastStackIndex];
            }
            else
            {
                newAbility.recastTime ??= new RecastData();
                newAbility.recastTime.recastStack = newAbility.name.ToUpper().Replace(" ", "_");
                GUILayout.Label("Ability Recast Time", EditorStyles.miniLabel);
                newAbility.recastTime.time = EditorGUILayout.FloatField("", newAbility.recastTime.time);
            }

            var effects = new HashSet<uint>();

            if (newAbility.effects.Length > 0)
                effects = newAbility.effects.ToHashSet();

            for (int i = 0; i < newAbility.effects.Length; i++)
            {
                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.Space(1.5f);
                GUILayout.Label("Effect #" + i, EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                newEffectIndex[i] = EditorGUILayout.Popup(newEffectIndex[i], availableEffects);

                int overflowIndex = newEffectIndex[i] - temporaryEffectDatabase.Data.Length;

                if (EditorGUI.EndChangeCheck())
                {
                    int tempId = newEffectIndex[i] >= temporaryEffectDatabase.Data.Length ? -1 : (int)temporaryEffectDatabase.Ids[newEffectIndex[i]];
                    int permId = tempId != -1 ? -1 : (int)permanentEffectDatabase.Ids[overflowIndex];
                    string id = tempId == -1 ? permId == -1 ? (-1).ToString() : permId.ToString() : tempId.ToString();
                    newAbility.effects[i] = uint.Parse(id);
                }

                var tempData = newEffectIndex[i] >= temporaryEffectDatabase.Data.Length ? null : temporaryEffectDatabase.Get(temporaryEffectDatabase.Ids[newEffectIndex[i]]);
                var permData = tempData != null ? null : permanentEffectDatabase.Get(permanentEffectDatabase.Ids[overflowIndex]);
                string name = tempData == null ? permData == null ? "" : permData.name : tempData.name;

                if (GUILayout.Button($"Remove {name} effect"))
                {
                    effects.Remove(newAbility.effects[i]);
                    newAbility.effects = effects.ToArray();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add more status effect"))
            {
                effects.Add(0);
                newAbility.effects = effects.ToArray();
                EditorGUILayout.EndVertical();
                return;
            }

            var potencies = new HashSet<AbilityPotency>();
            if (newAbility.potencies.Length > 0)
                potencies = newAbility.potencies.ToHashSet();

            for (int i = 0; i < newAbility.potencies.Length; i++)
            {
                GUILayout.Label("Ability Potency Type", EditorStyles.miniLabel);
                newAbility.potencies[i].type = (AbilityPotencyType)EditorGUILayout.EnumPopup(newAbility.potencies[i].type);
                GUILayout.Label("Ability Potency", EditorStyles.miniLabel);
                newAbility.potencies[i].potency = EditorGUILayout.IntField(newAbility.potencies[i].potency);

                if (GUILayout.Button("Remove Ability Potency"))
                {
                    potencies.Remove(newAbility.potencies[i]);
                    newAbility.potencies = potencies.ToArray();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }

            if (GUILayout.Button("Add more potency"))
            {
                potencies.Add(new AbilityPotency(AbilityPotencyType.Flat, 1));
                newAbility.potencies = potencies.ToArray();
                EditorGUILayout.EndVertical();
                return;
            }

            if (GUILayout.Button("Add Ability"))
            {
                database.Add(newId, newAbility);
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Search For Ability", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();
            EditorGUI.BeginChangeCheck();
            searchKeyword = EditorGUILayout.TextField(searchKeyword, EditorStyles.toolbarSearchField);

            if (EditorGUI.EndChangeCheck())
            {
                effectIndexForEdit = new int[database.Data.Length];
                availableEffectIdsForEdit = new uint[database.Data.Length][];
                availableEffectNamesForEdit = new string[database.Data.Length][];

                for (int i = 0; i < database.Data.Length; i++)
                {
                    if (string.IsNullOrEmpty(searchKeyword))
                        break;

                    var effectNameList = new HashSet<string>();

                    for (int j = 0; j < database.Data[i].effects.Length; j++)
                    {
                        string tempEffectName = temporaryEffectDatabase.Get(database.Data[i].effects[j])?.name;
                        string permEffectName = permanentEffectDatabase.Get(database.Data[i].effects[j])?.name;

                        if (!string.IsNullOrEmpty(tempEffectName) && tempEffectName.ToLower().Contains(searchKeyword.ToLower()))
                            effectNameList.Add(tempEffectName);

                        if (!string.IsNullOrEmpty(permEffectName) && permEffectName.ToLower().Contains(searchKeyword.ToLower()))
                            effectNameList.Add(permEffectName);
                    }

                    bool hasName = database.Data[i].name.ToLower().Contains(searchKeyword) || database.Data[i].desc.ToLower().Contains(searchKeyword);
                    bool hasEffect = effectNameList.Count > 0 && effectNameList.Any(e => e.ToLower().Contains(searchKeyword.ToLower()));

                    if (!hasName && !hasEffect)
                        continue;

                    var allEffectIdHash = new HashSet<uint>();
                    var allEffectNameHash = new List<string>();

                    for (int j = 0; j < temporaryEffectDatabase.Ids.Length; j++)
                    {
                        allEffectIdHash.Add(temporaryEffectDatabase.Ids[j]);
                        allEffectNameHash.Add(temporaryEffectDatabase.Data[j].name);
                    }

                    for (int j = 0; j < permanentEffectDatabase.Ids.Length; j++)
                    {
                        allEffectIdHash.Add(permanentEffectDatabase.Ids[j]);
                        allEffectNameHash.Add(permanentEffectDatabase.Data[j].name);
                    }

                    for (int j = 0; j < database.Data[i].effects.Length; j++)
                    {
                        var tempEffect = temporaryEffectDatabase.Get(database.Data[i].effects[j]);
                        var permEffect = permanentEffectDatabase.Get(database.Data[i].effects[j]);

                        if (tempEffect != null)
                        {
                            effectIndexForEdit[i] = j;
                            allEffectNameHash.Remove(tempEffect.name);
                            allEffectIdHash.Remove(database.Data[i].effects[j]);
                            continue;
                        }

                        if (permEffect != null)
                        {
                            effectIndexForEdit[i] = j;
                            allEffectNameHash.Remove(permEffect.name);
                            allEffectIdHash.Remove(database.Data[i].effects[j]);
                        }
                    }

                    availableEffectIdsForEdit[i] = allEffectIdHash.ToArray();
                    availableEffectNamesForEdit[i] = allEffectNameHash.ToArray();
                }
            }

            for (int i = 0; i < database.Data.Length; i++)
            {
                if (string.IsNullOrEmpty(searchKeyword))
                    break;

                var effectNameList = new HashSet<string>();
                var allEffectNameHash = new HashSet<string>();

                for (int j = 0; j < temporaryEffectDatabase.Ids.Length; j++)
                    allEffectNameHash.Add(temporaryEffectDatabase.Ids[j].ToString());

                for (int j = 0; j < permanentEffectDatabase.Ids.Length; j++)
                    allEffectNameHash.Add(permanentEffectDatabase.Ids[j].ToString());

                var allEffectName = allEffectNameHash.ToArray();

                for (int j = 0; j < database.Data[i].effects.Length; j++)
                {
                    string tempEffectName = temporaryEffectDatabase.Get(database.Data[i].effects[j])?.name;
                    string permEffectName = permanentEffectDatabase.Get(database.Data[i].effects[j])?.name;

                    if (!string.IsNullOrEmpty(tempEffectName) && tempEffectName.ToLower().Contains(searchKeyword.ToLower()))
                        effectNameList.Add(tempEffectName);

                    if (!string.IsNullOrEmpty(permEffectName) && permEffectName.ToLower().Contains(searchKeyword.ToLower()))
                        effectNameList.Add(permEffectName);
                }

                bool hasName = database.Data[i].name.ToLower().Contains(searchKeyword) || database.Data[i].desc.ToLower().Contains(searchKeyword);
                bool hasEffect = effectNameList.Count > 0 && effectNameList.Any(e => e.ToLower().Contains(searchKeyword.ToLower()));

                if (!hasName && !hasEffect)
                    continue;

                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.BeginVertical("Button");
                GUILayout.Label(database.Data[i].name, EditorStyles.largeLabel);
                EditorGUILayout.EndVertical();
                GUILayout.Label("Description", EditorStyles.miniLabel);
                database.Data[i].desc = EditorGUILayout.TextField(database.Data[i].desc);
                GUILayout.Label("Format Description", EditorStyles.miniLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(string.Format(database.Data[i].desc, database.Data[i].potencies.Length > 0 ?
                    database.Data[i].potencies[0].potency + (database.Data[i].potencies[0].type == AbilityPotencyType.Percentage ? "%" : "") : "Not Found"));
                GUILayout.Label("Status Effects", EditorStyles.miniLabel);
                EditorGUI.EndDisabledGroup();

                int idIndexForEdit = -1;
                var effectIdHash = new HashSet<uint>();

                if (database.Data[i].effects.Length <= 0)
                    effectIdHash = database.Data[i].effects.ToHashSet();

                for (int j = 0; j < database.Data[i].effects.Length; j++)
                {
                    var tempEffect = temporaryEffectDatabase.Get(database.Data[i].effects[j]);
                    var permEffect = permanentEffectDatabase.Get(database.Data[i].effects[j]);

                    if (tempEffect != null)
                    {
                        if (idIndexForEdit == -1)
                            idIndexForEdit = temporaryEffectDatabase.Ids.ToList().FindIndex(id => id == database.Data[i].effects[j]);

                        EditorGUILayout.BeginVertical("Button");
                        EditorGUILayout.Space(1.5f);
                        GUILayout.Label(tempEffect.name, EditorStyles.miniLabel);
                        EditorGUILayout.Space(1.5f);
                        EditorGUI.BeginChangeCheck();
                        idIndexForEdit = EditorGUILayout.Popup(idIndexForEdit, allEffectName);
                        if (EditorGUI.EndChangeCheck())
                        {
                            database.Data[i].effects[j] = temporaryEffectDatabase.Ids[idIndexForEdit];
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorUtility.SetDirty(database);
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField(string.Format(tempEffect.desc, tempEffect.potencies.Length > 0 ? tempEffect.potencies[0].potency + (tempEffect.potencies[0].type == EffectPotencyType.Percentage ? "%" : "") : "Not Found"));
                        GUILayout.Label($"Duration (seconds)", EditorStyles.miniLabel);
                        tempEffect.duration = EditorGUILayout.FloatField(tempEffect.duration);

                        if (tempEffect.potencies.Length > 0)
                        {
                            GUILayout.Label("Status Effect Potencies", EditorStyles.miniLabel);
                            for (int k = 0; k < tempEffect.potencies.Length; k++)
                            {
                                tempEffect.potencies[k].type = (EffectPotencyType)EditorGUILayout.EnumPopup(tempEffect.potencies[k].type);
                                tempEffect.potencies[k].potency = EditorGUILayout.FloatField(tempEffect.potencies[k].potency);
                            }
                        }

                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("Remove " + tempEffect.name))
                        {
                            effectIdHash.Remove(database.Data[i].effects[j]);
                            database.Data[i].effects = effectIdHash.ToArray();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorUtility.SetDirty(database);
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUILayout.Space(1.5f);
                        EditorGUILayout.EndVertical();
                    }

                    if (permEffect != null)
                    {
                        if (idIndexForEdit == -1)
                            idIndexForEdit = permanentEffectDatabase.Ids.ToList().FindIndex(id => id == database.Data[i].effects[j]);

                        EditorGUILayout.BeginVertical("Button");
                        EditorGUILayout.Space(1.5f);
                        GUILayout.Label(permEffect.name, EditorStyles.miniLabel);
                        EditorGUILayout.Space(1.5f);
                        EditorGUI.BeginChangeCheck();
                        idIndexForEdit = EditorGUILayout.Popup(idIndexForEdit, allEffectName);
                        if (EditorGUI.EndChangeCheck())
                        {
                            idIndexForEdit -= temporaryEffectDatabase.Ids.Length;
                            database.Data[i].effects[j] = permanentEffectDatabase.Ids[idIndexForEdit];
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorUtility.SetDirty(database);
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField(string.Format(permEffect.desc, permEffect.potencies.Length > 0 ? permEffect.potencies[0].potency + (permEffect.potencies[0].type == EffectPotencyType.Percentage ? "%" : "") : "Not Found"));

                        if (permEffect.potencies.Length > 0)
                        {
                            GUILayout.Label("Status Effect Potencies", EditorStyles.miniLabel);
                            for (int k = 0; k < permEffect.potencies.Length; k++)
                            {
                                permEffect.potencies[k].type = (EffectPotencyType)EditorGUILayout.EnumPopup(permEffect.potencies[k].type);
                                permEffect.potencies[k].potency = EditorGUILayout.FloatField(permEffect.potencies[k].potency);
                            }
                        }

                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("Remove " + permEffect.name))
                        {
                            effectIdHash.Remove(database.Data[i].effects[j]);
                            database.Data[i].effects = effectIdHash.ToArray();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorUtility.SetDirty(database);
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUILayout.Space(1.5f);
                        EditorGUILayout.EndVertical();
                    }
                }

                if (availableEffectIdsForEdit[i].Length > 0)
                {
                    GUILayout.Label("Add Ability", EditorStyles.miniLabel);
                    EditorGUILayout.BeginVertical("Button");
                    EditorGUILayout.Space(1.5f);
                    effectIndexForEdit[i] = EditorGUILayout.Popup(effectIndexForEdit[i], availableEffectNamesForEdit[i]);

                    if (GUILayout.Button("Add Ability Status Effect"))
                    {
                        var effectHash = database.Data[i].effects.ToHashSet();
                        effectHash.Add(availableEffectIdsForEdit[i][effectIndexForEdit[i]]);
                        database.Data[i].effects = effectHash.ToArray();
                        EditorUtility.SetDirty(database);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    EditorGUILayout.Space(1.5f);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Label("Ability Potencies", EditorStyles.miniLabel);

                for (int j = 0; j < database.Data[i].potencies.Length; j++)
                {
                    EditorGUILayout.BeginVertical("Button");
                    EditorGUILayout.Space(1.5f);
                    GUILayout.Label("Type", EditorStyles.miniLabel);
                    EditorGUI.BeginChangeCheck();
                    database.Data[i].potencies[j].type = (AbilityPotencyType)EditorGUILayout.EnumPopup(database.Data[i].potencies[j].type);
                    GUILayout.Label("Amount", EditorStyles.miniLabel);
                    database.Data[i].potencies[j].potency = EditorGUILayout.IntField(database.Data[i].potencies[j].potency);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(database);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        return;
                    }

                    if (GUILayout.Button("Remove Potency #" + (i + 1)))
                    {
                        var potencyList = database.Data[i].potencies.ToList();
                        potencyList.RemoveAt(j);
                        database.Data[i].potencies = potencyList.ToArray();
                        EditorUtility.SetDirty(database);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                        return;
                    }

                    EditorGUILayout.Space(1.5f);
                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Add Potency"))
                {
                    var potencyList = database.Data[i].potencies.ToList();
                    potencyList.Add(new AbilityPotency(AbilityPotencyType.Flat, 0));
                    database.Data[i].potencies = potencyList.ToArray();
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.Space(1.5f);
                if (GUILayout.Button("Remove " + database.Data[i].name))
                {
                    database.Remove(database.Ids[i]);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.Space(1.5f);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }
    }
}