using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.playbux.ability.editor
{
    [CustomEditor(typeof(ItemAbilityDatabase))]
    public class ItemToAbilityDatabaseEditor : Editor
    {
        private bool debugMode;
        private string searchKeyword;

        private string[] abilityNames;
        private int abilityIdIndex;
        private ItemAbilityDatabase database;

        private void OnEnable()
        {
            searchKeyword = "";
            database = (ItemAbilityDatabase)target;
            var ids = database.AbilityDatabase.Ids;
            abilityNames = new string[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                string name = $"ID {ids[i]} Not Found";
                if (database.AbilityDatabase.HasKey(ids[i]))
                    name =  $"[{ids[i]}] {database.AbilityDatabase.Get(ids[i]).name}";

                abilityNames[i] = name;
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

            GUILayout.Label("Target Ability", EditorStyles.miniLabel);
            abilityIdIndex = EditorGUILayout.Popup(abilityIdIndex, abilityNames);

            EditorGUILayout.EndVertical();
        }
    }
}