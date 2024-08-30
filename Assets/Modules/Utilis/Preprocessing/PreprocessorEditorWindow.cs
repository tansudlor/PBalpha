using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.Collections.Generic;

namespace com.playbux.utilis.preprocessing
{
    public class PreprocessorEditorWindow : EditorWindow
    {
        private string projectPath => "Assets/Modules/Utilis/" + buildPreprocessor;
        private string fullPath => Application.dataPath + "/Modules/Utilis/" + buildPreprocessor;

        private const string buildPreprocessor = "Preprocessor.txt";

        private bool tryLoad;

        private bool serverMode;

        private bool initializedConstant;

        private int buildTargetIndex;

        private Vector2 scrollPos = Vector2.zero;

        private string[] buildTargetText;

        private List<BuildTargetData> buildTarget;

        private List<PreprocessorData> preprocessorDatas;


        [MenuItem("Playbux/Utilis/Preprocessor")]
        public static void Show()
        {
            var window = GetWindow<PreprocessorEditorWindow>();
            window.titleContent = new GUIContent("Preprocessor Management");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Scripting Preprocessor Define Management", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            preprocessorDatas ??= new List<PreprocessorData>();
            LoadAsset();
            LoadConstant();

            serverMode = EditorGUILayout.ToggleLeft("Is Server?", serverMode);

            GUILayout.Label("Build Group");

            for (int i = 0; i < buildTarget.Count; i++)
            {
                buildTarget[i].enabled = EditorGUILayout.ToggleLeft(buildTarget[i].target.ToString(), buildTarget[i].enabled);
            }

            EditorGUILayout.Space(5);

            GUILayout.Label("Preprocessors");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < preprocessorDatas.Count; i++)
            {
                preprocessorDatas[i].enabled = EditorGUILayout.ToggleLeft("Enabled?", preprocessorDatas[i].enabled);
                preprocessorDatas[i].name = EditorGUILayout.TextField("#" + i, preprocessorDatas[i].name);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Preprocessor", EditorStyles.miniButton))
                preprocessorDatas.Add(new PreprocessorData());

            if (GUILayout.Button("Save", EditorStyles.miniButton))
                SavePreprocessor();

            EditorGUILayout.Space(2.5f);
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Build With Pre-defined Scripting", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            buildTargetIndex = EditorGUILayout.Popup(buildTargetIndex, buildTargetText);

            if (GUILayout.Button("Build"))
                Build();
        }

        private void LoadConstant()
        {
            if (initializedConstant)
                return;

            buildTarget ??= new List<BuildTargetData>();

            //NOTE: for server group
            buildTarget.Add(new BuildTargetData { enabled = false, target = BuildTarget.StandaloneWindows64 });
            buildTarget.Add(new BuildTargetData { enabled = false, target = BuildTarget.StandaloneOSX });
            buildTarget.Add(new BuildTargetData { enabled = false, target = BuildTarget.iOS, iOSSubtarget = iOSTargetDevice.iPhoneAndiPad });
            buildTarget.Add(new BuildTargetData { enabled = false, target = BuildTarget.Android, androidSubtarget = AndroidTargetDevices.PhonesTabletsAndTVDevicesOnly });

            buildTargetText = buildTarget.Select(bg => bg.target.ToString()).ToArray();

            initializedConstant = true;
        }

        private void LoadAsset()
        {
            if (tryLoad)
                return;

            var isFileExist = File.Exists(fullPath);

            if (!isFileExist)
            {
                File.WriteAllText(fullPath, "");
                AssetDatabase.ImportAsset(projectPath);
            }

            try
            {
                var reader = new StreamReader(fullPath);
                var json = reader.ReadToEnd();
                preprocessorDatas = JsonUtility.FromJson<PreprocessorWrapper>(json).preprocessors.ToList();
                reader.Close();
                reader.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            tryLoad = true;
        }

        private void Build()
        {
            string path = "";
            string defaultPath = Application.persistentDataPath;

#if UNITY_EDITOR_WIN
            defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif

            path = EditorUtility.SaveFolderPanel("Build Location", defaultPath, "");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Error", "path is not found", "Dismiss");
                return;
            }

            LoadAsset();

            bool hasDevelopmentTag = false;
            List<string> defines = new List<string>();

            for (int i = 0; i < preprocessorDatas.Count; i++)
            {
                if (!preprocessorDatas[i].enabled)
                    continue;

                defines.Add(preprocessorDatas[i].name);

                if (preprocessorDatas[i].name == "DEVELOPMENT")
                    hasDevelopmentTag = true;
            }

            if (serverMode)
                defines.Add("SERVER");


            var target = buildTarget[buildTargetIndex].target;
            var targetGroup = BuildTargetGroup.Standalone;

            if (target is BuildTarget.StandaloneLinux64 or BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64)
                targetGroup = BuildTargetGroup.Standalone;

            if (target is BuildTarget.iOS)
                targetGroup = BuildTargetGroup.iOS;

            if (target is BuildTarget.Android)
                targetGroup = BuildTargetGroup.Android;

            Debug.Log(string.Join(";", defines));

            if (hasDevelopmentTag)
            {
                PlayerSettings.defaultScreenWidth = 800;
                PlayerSettings.defaultScreenHeight = 400;
                PlayerSettings.allowFullscreenSwitch = true;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            }
            else
            {
                PlayerSettings.allowFullscreenSwitch = true;
                PlayerSettings.defaultIsNativeResolution = true;
                PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = path + "/Playbux.exe";
            buildPlayerOptions.targetGroup = targetGroup;
            buildPlayerOptions.subtarget = serverMode ? 1 : 0;
            buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer;
            buildPlayerOptions.target = buildTarget[buildTargetIndex].target;
            buildPlayerOptions.extraScriptingDefines = defines.ToArray();
            buildPlayerOptions.scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private void SavePreprocessor()
        {
            var wrapper = new PreprocessorWrapper
            {
                preprocessors = preprocessorDatas.ToArray()
            };

            var json = JsonUtility.ToJson(wrapper);
            var write = new StreamWriter(fullPath);
            write.Write(json);
            write.Flush();
            write.Close();
            write.Dispose();

            var defines = "";

            for (int i = 0; i < preprocessorDatas.Count; i++)
            {
                if (!preprocessorDatas[i].enabled)
                    continue;

                defines += preprocessorDatas[i].name;
                defines += ";";
            }

            if (serverMode)
            {
                defines += "SERVER";
                defines += ";";
            }

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, defines);

            for (int i = 0; i < buildTarget.Count; i++)
            {
                if (!buildTarget[i].enabled)
                    continue;

                var target = buildTarget[i].target;
                var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

                if (target is BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneOSX or BuildTarget.StandaloneLinux64)
                    targetGroup = BuildTargetGroup.Standalone;

                if (target is BuildTarget.iOS)
                    targetGroup = BuildTargetGroup.iOS;

                if (target is BuildTarget.Android)
                    targetGroup = BuildTargetGroup.Android;

                var sTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

                defines = "";

                for (int j = 0; j < preprocessorDatas.Count; j++)
                {
                    if (!preprocessorDatas[j].enabled)
                        continue;

                    defines += preprocessorDatas[j].name;
                    defines += ";";
                    Debug.Log(sTarget.TargetName + " " + preprocessorDatas[j].name);
                }

                if (serverMode && sTarget == NamedBuildTarget.Standalone)
                {
                    defines += "SERVER";
                    defines += ";";
                }

                PlayerSettings.SetScriptingDefineSymbols(sTarget, defines);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(projectPath);
        }
    }
}