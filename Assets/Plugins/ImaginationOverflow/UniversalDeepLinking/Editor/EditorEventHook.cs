using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImaginationOverflow.UniversalDeepLinking;
using ImaginationOverflow.UniversalDeepLinking.Editor;
using ImaginationOverflow.UniversalDeepLinking.Providers;
using ImaginationOverflow.UniversalDeepLinking.Storage;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.ImaginationOverflow.UniversalDeepLinking.Editor
{
    [InitializeOnLoad]
    public class EditorEventHook
    {
        static EditorEventHook()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange obj)
        {
            if (obj != PlayModeStateChange.ExitingPlayMode)
                return;

            if (ConfigurationStorage.Load().StandaloneWindowsLinuxAutoDeferralForDebug == false)
                return;


            var path = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), EditorHelpers.LauncherExePluginPath);

            var exePath = path.Replace(".prog", ".exe");
            if (File.Exists(exePath))
                File.Delete(exePath);

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void SetupEditorDeepLink()
        {
            EditorHelpers.SetPluginName("UniversalDeepLinking");
            var path = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), EditorHelpers.LauncherPluginFolder);

            EditorLinkProvider.WindowsProviderForEditorLauncher = new WindowsLauncherLinkProvider(false);

            ProviderHelpers.SetWindowsMockLauncherLocation(path);



            if (ConfigurationStorage.Load().StandaloneWindowsLinuxAutoDeferralForDebug == false)
                return;

            path = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), EditorHelpers.LauncherExePluginPath);

            var exePath = path.Replace(".prog", ".exe");
            if (File.Exists(exePath) == false)
                File.Copy(path, exePath);
        }

    }
}