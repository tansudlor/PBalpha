using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if STANDALONE_HELPERS
namespace ImaginationOverflow.UniversalDeepLinking.Helpers
#else
namespace ImaginationOverflow.UniversalDeepLinking
#endif
{
    public static class ProviderHelpers
    {
        public const string LauncherRelativeFolder = "ImaginationOverflow/UniversalDeepLink/";
        public const string LauncherName = "UDLLauncher.exe";
        public const string LauncherConfigFileName = LauncherName + ".config";
        public const string LauncherSharedFileName = "udlsharedfile.dat";


#if !UDL_DLL_BUILD

        public const string WindowsConfigFileFormat = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
	<startup>
		<supportedRuntime version=""v4.0""/>
		<supportedRuntime version=""v2.0.50727""/>
	</startup>
	<appSettings>
		<add key=""Exec"" value=""{0}"" />
		<add key=""Steam"" value=""{1}"" />
		<add key=""SteamPath"" value=""{2}"" />
		<add key=""SharedFile"" value=""{3}"" />
	</appSettings>
</configuration>";

        private static string _editorLauncherFolder;

        public static void SetWindowsMockLauncherLocation(string launcherPluginFolder)
        {
            _editorLauncherFolder = launcherPluginFolder.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string GetLauncherFolder()
        {
            if (string.IsNullOrEmpty(_editorLauncherFolder))
                return LauncherFolder();

            return _editorLauncherFolder;
        }

        private static string LauncherFolder()
        {
            return Path.Combine(Application.streamingAssetsPath.Replace('/', Path.DirectorySeparatorChar), LauncherRelativeFolder);
        }



        public static string GetLauncherFullFilename()
        {
            return Path.Combine(GetLauncherFolder(), LauncherName);
        }

        public static string GetLauncherConfigFileDirectory()
        {
#if UNITY_EDITOR
            return GetLauncherFolder();
#else
            return Application.persistentDataPath;
#endif
        }

        public static string GetLauncherConfigFullFilename()
        {
            return Path.Combine(GetLauncherConfigFileDirectory(), LauncherConfigFileName);
        }

        public static string GetLauncherSharedFileFullFilename()
        {
            return Path.Combine(GetLauncherConfigFileDirectory(), LauncherSharedFileName);
        }

#endif





        public static string GetExecutingPath()
        {

            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                // ignored
            }

            var exe = Environment.GetCommandLineArgs()[0];

            var parentDataDir = Directory.GetParent(Application.dataPath).FullName;

            if (exe.Contains("/"))
                parentDataDir = parentDataDir.Replace('\\', Path.DirectorySeparatorChar);
            else
                parentDataDir = parentDataDir.Replace('/', Path.DirectorySeparatorChar);

            if (exe.StartsWith(parentDataDir))
                return exe;

            return Path.Combine(parentDataDir, Path.GetFileName(exe));
        }
    }
}