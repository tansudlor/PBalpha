using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.PreBuild
{
    public class PreBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return int.MaxValue - 10; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("PreBuild");
            EditorHelpers.SetPluginName("UniversalDeepLinking");

            var processor = GetProcessor(target);

            if (processor == null)
                return;


            AppLinkingConfiguration configuration = Storage.ConfigurationStorage.Load();


            processor.OnPreprocessBuild(configuration, path);
        }

        private static IPreBuild GetProcessor(BuildTarget target)
        {
            if (target == BuildTarget.Android)
                return new AndroidPreBuild();


#if !UDL_DLL_BUILD
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                HandleAutoLauncherSet();
            }
#endif
#if UDL_MACOS_NATIVE_DLL
            HandleMacIl2cpp();
#endif
            return null;
        }

        private static void HandleAutoLauncherSet()
        {
#if !UDL_DLL_BUILD
            ProviderHelpers.SetWindowsMockLauncherLocation("");
            AppLinkingConfiguration c = Storage.ConfigurationStorage.Load();

            if (c.StandaloneWindowsLinuxAutoDeferral == false)
            {
                if (Directory.Exists(ProviderHelpers.GetLauncherFolder()))
                    Directory.Delete(ProviderHelpers.GetLauncherFolder(), true);

                return;
            }

            var launcherPath = EditorHelpers.LauncherExePluginPath;

            var launcherFullPath = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), launcherPath);

            if (Directory.Exists(ProviderHelpers.GetLauncherFolder()) == false)
                Directory.CreateDirectory(ProviderHelpers.GetLauncherFolder());


            if (File.Exists(ProviderHelpers.GetLauncherFullFilename()))
                File.Delete(ProviderHelpers.GetLauncherFullFilename());

            File.Copy(launcherFullPath.Replace('/',Path.DirectorySeparatorChar), ProviderHelpers.GetLauncherFullFilename());
#endif
        }

        private static void HandleMacIl2cpp()
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone)
                return;

            var isIl2cpp = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) ==
                           ScriptingImplementation.IL2CPP;

            //#if UDL_DLL_BUILD
            //            if (isIl2cpp)
            //            {
            //                Debug.LogWarning("Il2cpp selected, please ensure the following: " +
            //                                 "\nThe UDLMacIL2CPP.dylib is set to be included in the compilation for MacOs (by default it isn't)" +
            //                                 "\nThe ImaginationOverflow.UniversalDeepLinking.Platform library under /Plugins/ImaginationOverflow/UniversalDeepLinking/libs/Standalone/Mac is not set to be included on this build" +
            //                                 "\nThe ImaginationOverflow.UniversalDeepLinking.Platform library under /Plugins/ImaginationOverflow/UniversalDeepLinking/libs/Standalone/Mac/il2cpp is set to be included on this build ");
            //            }
            //#endif
            var staticLibPath = EditorHelpers.AssetPluginPath + "/libs/Standalone/Mac/UDLMacIL2CPP.dylib";

            var staticLib = (AssetImporter.GetAtPath(staticLibPath) as PluginImporter);
#if UDL_DLL_BUILD
            var dll = (AssetImporter.GetAtPath(EditorHelpers.AssetPluginPath +
                                               "/libs/Standalone/Mac/il2cpp/ImaginationOverflow.UniversalDeepLinking.Platform.dll")
                as PluginImporter);
            var originalDll = (AssetImporter.GetAtPath(EditorHelpers.AssetPluginPath +
                                                       "/libs/Standalone/Mac/ImaginationOverflow.UniversalDeepLinking.Platform.dll")
                as PluginImporter);
            SetIl2cppConfig(false, dll);
            SetIl2cppConfig(false, originalDll);
            SetIl2cppConfig(isIl2cpp, dll);
            SetIl2cppConfig(!isIl2cpp, originalDll);
#endif

            if (staticLib != null)
            {
                SetIl2cppConfig(isIl2cpp, staticLib);
            }

            return;
        }

        private static void SetIl2cppConfig(bool isCpp, PluginImporter dll)
        {
#if !UNITY_2018_1_OR_NEWER
            var target = BuildTarget.StandaloneOSX;
#else
            var target = BuildTarget.StandaloneOSX;
#endif
            dll.ClearSettings();
            dll.SetCompatibleWithAnyPlatform(false);

            if (isCpp)
            {
                dll.SetCompatibleWithPlatform(target, true); //StandaloneOSX
            }

            dll.SaveAndReimport();
        }
    }
}