using UnityEditor;
using UnityEngine;

namespace com.playbux.fakeplayer.editor
{
    [CustomEditor(typeof(FakePlayerAssetDatabase))]
    public class FakePlayerAssetDatabaseEditor : Editor
    {
        private bool debugMode;
        private string searchKeyword;
        private FakePlayerAssetDatabase assetDatabase;

        private void OnEnable()
        {
            assetDatabase = (FakePlayerAssetDatabase)target;
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
                assetDatabase.AssignAssetId();
                EditorUtility.SetDirty(assetDatabase);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
