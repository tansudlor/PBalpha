#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace com.playbux.tool
{
    public class TextInputDialog : EditorWindow
    {
        private System.Action<string> onSubmit;
        private static string text;
        public static void OpenDialog(System.Action<string> onSubmit, string text_init = "")
        {
            var window = GetWindow<TextInputDialog>("Enter Text");
            text = text_init;
            window.onSubmit = onSubmit;
            window.minSize = new Vector2(400, 170);
            window.maxSize = new Vector2(400, 170);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Enter your text:", EditorStyles.boldLabel);
            text = EditorGUILayout.TextArea(text, GUILayout.Height(100));
            
            if (GUILayout.Button("Submit"))
            {
                onSubmit?.Invoke(text);
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }
    }
}
#endif