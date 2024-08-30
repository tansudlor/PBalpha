using UnityEditor;
using UnityEngine;

namespace com.playbux.enemy.editor
{
    [CustomEditor(typeof(EnemyAssetDatabase))]
    public class AbilityDatabaseEditor : Editor
    {
        private bool debugMode;
        private string searchKeyword;
        private EnemyAssetDatabase database;

        private void OnEnable()
        {
            database = (EnemyAssetDatabase)target;
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

            if (GUILayout.Button("Assign Asset Id"))
            {
                database.AssignAssetId();
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.EndVertical();
        }
    }
}