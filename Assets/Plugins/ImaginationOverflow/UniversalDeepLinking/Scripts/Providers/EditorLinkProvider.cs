using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Storage;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
    public class EditorLinkProvider : ILinkProvider
    {
        private static EditorLinkProvider _instance;

#if !UDL_DLL_BUILD
        public static ILinkProvider WindowsProviderForEditorLauncher;
#endif
        public EditorLinkProvider()
        {
            _instance = this;
        }

        public bool Initialize()
        {
            var config = ConfigurationStorage.Load();

#if !UDL_DLL_BUILD && UNITY_EDITOR_WIN

            if (config.StandaloneWindowsLinuxAutoDeferralForDebug)
            {
                var currConfig = config.StandaloneWindowsLinuxAutoDeferral;
                config.StandaloneWindowsLinuxAutoDeferral = true;
                ConfigurationStorage.Save(config);
                try
                {
                    var res = WindowsProviderForEditorLauncher.Initialize();

                    if (res)
                        WindowsProviderForEditorLauncher.LinkReceived += (link) => { SimulateLink(link); };

                    return res;
                }
                catch (Exception e)
                {
                }
                finally
                {
                    config.StandaloneWindowsLinuxAutoDeferral = currConfig;
                    ConfigurationStorage.Save(config);
                }
            }
#endif
            return true;
        }

        public event Action<string> LinkReceived;

        public void PollInfoAfterPause()
        {
        }

        public static void SimulateLink(string link)
        {
            if (_instance == null)
                return;

            var evt = _instance.LinkReceived;
            if (evt != null)
                evt(link);
        }
    }
}