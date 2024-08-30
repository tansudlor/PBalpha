using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Editor.Xcode;
using ImaginationOverflow.UniversalDeepLinking.Providers;
using ImaginationOverflow.UniversalDeepLinking.Storage;
using UnityEditor;
using UnityEngine;


namespace ImaginationOverflow.UniversalDeepLinking.Editor.Ui
{
    public class DebugWindow : EditorWindow
    {
        private const int MaxDebugDeepLinkStorage = 10;
        private string _link;
        private AppLinkingConfiguration _config;

        [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/Debug", priority = 2)]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DebugWindow), false, "UDL Debug").Show();
        }

        private void OnEnable()
        {
        }

        void OnGUI()
        {
            EnsureConfig();
            var config = _config;
            EditorGUILayout.LabelField("  ");
            EditorGUILayout.LabelField("Type an URL or URI that you which to debug.");
            EditorGUILayout.LabelField("  ");
            _link = EditorGUILayout.TextField("Debug Link", _link);


            if (GUILayout.Button("Debug"))
            {
                Debug(_link);
            }


            if (config.DebugDeeplinks == null)
                return;

            GUILayout.Label("\n\nPrevious deep link activations:\n");
            for (int i = 0; i < config.DebugDeeplinks.Length; i++)
            {
                GUILayout.BeginHorizontal(GUIStyle.none);
                if (GUILayout.Button(">>", GUILayout.Width(60)))
                    Debug(config.DebugDeeplinks[i]);

                GUILayout.Label(config.DebugDeeplinks[i]);

                GUILayout.EndHorizontal();

            }


        }

        private void Debug(string link)
        {
            EditorLinkProvider.SimulateLink(link);

            if (_config.DebugDeeplinks == null)
                _config.DebugDeeplinks = new string[0];

            var arr = _config.DebugDeeplinks;

            if (arr.Length < MaxDebugDeepLinkStorage)
                Array.Resize(ref arr, arr.Length + 1);

            for (int i = arr.Length - 1; i > 0; i--)
            {
                arr[i] = arr[i - 1];
            }

            arr[0] = link;
            _config.DebugDeeplinks = arr;
            ConfigurationStorage.Save(_config);
            _config = null;
        }

        private void EnsureConfig()
        {
            if (_config != null)
                return;
            _config = ConfigurationStorage.Load();

        }
    }
}

