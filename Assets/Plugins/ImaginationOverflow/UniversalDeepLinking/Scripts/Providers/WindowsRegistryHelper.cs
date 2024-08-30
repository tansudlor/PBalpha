using System;
using System.IO;
#if !(NET_Standard_2_0 || NET_Standard || NET_STANDARD_2_0)
using Microsoft.Win32;
using UnityEngine;
#if UDL_DLL_BUILD && STANDALONE_HELPERS
using ImaginationOverflow.UniversalDeepLinking.Helpers;
namespace ImaginationOverflow.UniversalDeepLinking.Providers.Helpers
#else
namespace ImaginationOverflow.UniversalDeepLinking.Providers
#endif
{
    public class WindowsRegistryHelper
    {
        public static string GetSteamPath()
        {

            var steamExe = Registry.CurrentUser.OpenSubKey("Software", false)
                .OpenSubKey("Classes")
                .OpenSubKey("Steam")
                .OpenSubKey("shell")
                .OpenSubKey("open")
                .OpenSubKey("command")
                .GetValue("").ToString();
            steamExe = steamExe.Replace(" \"%1\"", string.Empty).Replace("\"", String.Empty);
            return steamExe;
        }

        public static bool SetProtocol(string activationProtocol, string executingExe, string exeArgs, string launcherArgs = "")
        {
            var key = Registry.CurrentUser.OpenSubKey("Software", true);
            var classes = key.OpenSubKey("Classes", true);

            if (classes == null)
                return false;

            var appkey = classes.OpenSubKey(activationProtocol);

            if (appkey != null)
            {
                classes.DeleteSubKeyTree(activationProtocol);
            }

            appkey = classes.CreateSubKey(activationProtocol);
            appkey.SetValue("", string.Format("URL:{0} Protocol", activationProtocol));
            appkey.SetValue("URL Protocol", "");

            appkey
                .CreateSubKey("shell")
                .CreateSubKey("open")
                .CreateSubKey("command")
                .SetValue("", string.Format("\"{0}\" {1} \"%1\" \"{2}\"", executingExe, exeArgs, launcherArgs));


            var defaultIcon = appkey.CreateSubKey("DefaultIcon");
            defaultIcon.SetValue("", string.Format("\"{0}\",-1", ProviderHelpers.GetExecutingPath()));

            try
            {
                var appKey = appkey.CreateSubKey("Application");
                appKey.SetValue("ApplicationName", Application.productName);
                Debug.Log("Registry Application set to " + Application.productName);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            return true;
        }

    }
}
#endif