using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ImaginationOverflow.UniversalDeepLinking.Storage;
using UnityEngine;
using WindowsRegistryHelper = ImaginationOverflow.UniversalDeepLinking.Providers.Helpers.WindowsRegistryHelper;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR


    public class WindowsLauncherLinkProvider : ILinkProvider
    {
        private readonly bool _steamBuild;
        private List<string> _schemes = new List<string>();

        public WindowsLauncherLinkProvider(bool steamBuild)
        {
            _steamBuild = steamBuild;
        }

        public bool Initialize()
        {
            return Initialize(ConfigurationStorage.Load());
        }

        public bool Initialize(AppLinkingConfiguration config)
        {
            var protocol = config.GetPlatformDeepLinkingProtocols(SupportedPlatforms.Windows, true).FirstOrDefault();

            if (protocol == null || string.IsNullOrEmpty(protocol.Scheme))
                return false;

            var fromSteam = _steamBuild;
            var steamAppId = config.SteamId;

            try
            {
                bool autoDeferral =
#if UDL_DLL_BUILD
                        false
#else
                        config.StandaloneWindowsLinuxAutoDeferral
#endif
                    ;

                var executablePath = GetExe(fromSteam, steamAppId, out string exeArgs, autoDeferral);


                foreach (var activationProtocol in config.GetPlatformDeepLinkingProtocols(SupportedPlatforms.Windows,
                             true))
                {
                    if (WindowsRegistryHelper.SetProtocol(activationProtocol.Scheme, executablePath, exeArgs) == false)
                        return false;

                    _schemes.Add(activationProtocol.Scheme);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }


        private string GetExe(bool fromSteam, string steamAppId, out string args, bool autoDeferral)
        {
            args = string.Empty;

#if !UDL_DLL_BUILD
            if (autoDeferral)
            {
                return SetAndGetAutoLauncher(fromSteam, steamAppId);
            }
#endif

            try
            {
                if (fromSteam && string.IsNullOrEmpty(steamAppId) == false)
                {
                    var steamExe = WindowsRegistryHelper.GetSteamPath();
                    args = " -applaunch " + steamAppId;
                    return steamExe;
                }
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(LinkProviderFactory.DeferredExePath) == false)
            {
                return LinkProviderFactory.DeferredExePath;
            }


            return ProviderHelpers.GetExecutingPath();
        }
#if !UDL_DLL_BUILD
        private string SetAndGetAutoLauncher(bool fromSteam, string steamAppId)
        {
            var currApp = ProviderHelpers.GetExecutingPath();
            var steamPath = "";
            if (fromSteam && string.IsNullOrEmpty(steamAppId) == false)
            {
                steamPath = WindowsRegistryHelper.GetSteamPath();
            }


            var sharedFileName = ProviderHelpers.GetLauncherSharedFileFullFilename();

            File.WriteAllText(ProviderHelpers.GetLauncherConfigFullFilename(),
                string.Format(ProviderHelpers.WindowsConfigFileFormat, currApp, steamAppId, steamPath, sharedFileName));


            File.WriteAllText(sharedFileName, string.Empty);

            var fw = new FileSystemWatcher(ProviderHelpers.GetLauncherFolder());
            fw.Filter = ProviderHelpers.LauncherSharedFileName;

            fw.EnableRaisingEvents = true;
            fw.Changed += (s, f) =>
            {
                var dp = File.ReadAllText(sharedFileName);

                if (string.IsNullOrEmpty(dp))
                    return;

                UniversalDeeplinkingRuntimeScript.Instance.Schedule(() => { OnLinkReceived(dp); });

                File.WriteAllText(sharedFileName, string.Empty);
            };

            return ProviderHelpers.GetLauncherFullFilename();
        }
#endif


        private event Action<string> _linkReceived;

        public event Action<string> LinkReceived
        {
            add
            {
                _linkReceived += value;
                CheckArguments();
            }
            remove { _linkReceived -= value; }
        }

        private void CheckArguments()
        {
            if (_schemes.Count == 0)
                return;


            var arg = Environment.GetCommandLineArgs().FirstOrDefault(a => _schemes.Any(a.StartsWith));

            if (string.IsNullOrEmpty(arg) == false)
                OnLinkReceived(arg);
        }

        public void PollInfoAfterPause()
        {
        }

        protected virtual void OnLinkReceived(string obj)
        {
            var handler = _linkReceived;
            if (handler != null) handler(obj);
        }
    }

#endif
}