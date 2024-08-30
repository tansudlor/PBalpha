#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.playbux.utilis
{
    public class BuildProject
    {
        static public string state = "free";
        [MenuItem("ServerClient/Set/Client")]
        public static void ClientSymbols()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + symbols);
            HashSet<string> hashSymbols = new HashSet<string>(symbols.Split(';'));
            hashSymbols.Remove("SERVER");
            hashSymbols.Add("CLIENT");
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + string.Join(';', hashSymbols.ToArray()));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(';', hashSymbols.ToArray()));
            AssetDatabase.Refresh();
        }
        [MenuItem("ServerClient/Set/Server")]
        public static void ServerSymbols()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + symbols);
            HashSet<string> hashSymbols = new HashSet<string>(symbols.Split(';'));
            hashSymbols.Remove("CLIENT");
            hashSymbols.Add("SERVER");
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + string.Join(';', hashSymbols.ToArray()));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(';', hashSymbols.ToArray()));
            AssetDatabase.Refresh();
        }

        [MenuItem("ServerClient/Set/Binary")]
        public static void BinarySymbols()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + symbols);
            HashSet<string> hashSymbols = new HashSet<string>(symbols.Split(';'));
            hashSymbols.Add("BINARY_NETWORK");
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + string.Join(';', hashSymbols.ToArray()));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(';', hashSymbols.ToArray()));
            AssetDatabase.Refresh();
        }


        [MenuItem("ServerClient/Set/Text")]
        public static void TextSymbols()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + symbols);
            HashSet<string> hashSymbols = new HashSet<string>(symbols.Split(';'));
            hashSymbols.Remove("BINARY_NETWORK");
            Debug.Log("Define Symbols for " + buildTargetGroup.ToString() + ": " + string.Join(';', hashSymbols.ToArray()));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(';', hashSymbols.ToArray()));
            AssetDatabase.Refresh();
        }

        [MenuItem("ServerClient/Run-Client")]
        public static void RunClient()
        {
            // หาเส้นทางของโฟลเดอร์ Assets
            string assetsPath = Application.dataPath;
            // ปรับเส้นทางตามที่คุณต้องการ
            string pathToExe = System.IO.Path.Combine(assetsPath, "../../Builds/Client/ClientBuild.exe");
            string fullPath = Path.GetFullPath(pathToExe);
            Debug.Log(fullPath);
            Process.Start(fullPath);
        }

        [MenuItem("ServerClient/Run-Server")]
        public static void RunServer()
        {
            // หาเส้นทางของโฟลเดอร์ Assets
            string assetsPath = Application.dataPath;
            // ปรับเส้นทางตามที่คุณต้องการ
            string pathToExe = System.IO.Path.Combine(assetsPath, "../../Builds/Server/ServerBuild.exe");
            string fullPath = Path.GetFullPath(pathToExe);
            Debug.Log(fullPath);
            Process.Start(fullPath);
        }

        [MenuItem("ServerClient/Build&Run/Client")]
        public static void BuildClient()
        {
            string symbols = "DEVELOPMENT;MIRROR;MIRROR_57_0_OR_NEWER;MIRROR_58_0_OR_NEWER;MIRROR_65_0_OR_NEWER;MIRROR_66_0_OR_NEWER;MIRROR_2022_9_OR_NEWER;MIRROR_2022_10_OR_NEWER;MIRROR_70_0_OR_NEWER;MIRROR_71_0_OR_NEWER;MIRROR_73_OR_NEWER;MIRROR_78_OR_NEWER;MIRROR_79_OR_NEWER;MIRROR_81_OR_NEWER;CLIENT";
            string path = "../Builds/Client";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path + "/ClientBuild.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.AutoRunPlayer,
                extraScriptingDefines = symbols.Split(';')
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        [MenuItem("ServerClient/Build&Run/Server")]
        public static void BuildServer()
        {
            string symbols = "DEVELOPMENT;MIRROR;MIRROR_57_0_OR_NEWER;MIRROR_58_0_OR_NEWER;MIRROR_65_0_OR_NEWER;MIRROR_66_0_OR_NEWER;MIRROR_2022_9_OR_NEWER;MIRROR_2022_10_OR_NEWER;MIRROR_70_0_OR_NEWER;MIRROR_71_0_OR_NEWER;MIRROR_73_OR_NEWER;MIRROR_78_OR_NEWER;MIRROR_79_OR_NEWER;MIRROR_81_OR_NEWER;SERVER";
            string path = "../Builds/Server";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path + "/ServerBuild.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.AutoRunPlayer,
                extraScriptingDefines = symbols.Split(';')
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);

        }

    }
}
#endif
