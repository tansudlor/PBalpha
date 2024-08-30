using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Editor.Ui;
using UnityEditor;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.Ui
{
    internal class AboutWindow : EditorWindow
    {
        [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/About", priority = 10)]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AboutWindow), false, "About").Show();
        }

        private void OnEnable()
        {
        }

        void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15 };

            EditorGUILayout.LabelField("Universal Deep Linking", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("v2.4.1", style, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("", style, GUILayout.ExpandWidth(true));

            var button = style;
            button.richText = true;
            EditorGUILayout.LabelField("Other Plugins:", style, GUILayout.ExpandWidth(true));
            button.fontStyle = FontStyle.Bold;



            SetPlugin("Universal Deep Linking", "https://assetstore.unity.com/packages/slug/243184?aid=1100l8RBa", "https://assetstore.unity.com/packages/slug/243183?aid=1100l8RBa");
            SetPlugin("Universal File Association", "https://assetstore.unity.com/packages/slug/155366?aid=1100l8RBa", "https://assetstore.unity.com/packages/slug/158709?aid=1100l8RBa");
            SetPlugin("Deferred Deep Linking for Android", "https://assetstore.unity.com/packages/slug/161691?aid=1100l8RBa", "https://assetstore.unity.com/packages/slug/161692?aid=1100l8RBa");
            SetPlugin("Share Target for Android", "https://assetstore.unity.com/packages/slug/176964?aid=1100l8RBa", "https://assetstore.unity.com/packages/slug/176965?aid=1100l8RBa");

            if (GUILayout.Button("<color=#4553E7>@ImaginationOverflow</color>", button))
            {
                Application.OpenURL("http://imaginationoverflow.com/");
            }
        }

        private void SetPlugin(string name, string reg, string pro)
        {
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15 };
            var button = style;
            button.richText = true;
            button.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField(name, style, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<color=#4553E7>PRO</color>", button))
            {
                Application.OpenURL(pro);
            }

            if (GUILayout.Button("<color=#4553E7>Regular</color>", button))
            {
                Application.OpenURL(reg);
            }



            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");

        }
    }
}
