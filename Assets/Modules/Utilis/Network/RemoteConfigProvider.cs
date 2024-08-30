using System.Collections.Generic;
using Zenject;
using UnityEngine;
using com.playbux.events;
using Unity.Services.Core;
using Cysharp.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;

namespace com.playbux
{
    public class RemoteConfigProvider : IInitializable, ILateDisposable
    {
        public struct UserAttributes
        {

        }

        public struct AppAttributes
        {

        }

        private readonly SignalBus signalBus;

        private Dictionary<string, int> requestedVariables;

        public RemoteConfigProvider(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            requestedVariables = new Dictionary<string, int>();
        }

        public void Initialize()
        {
            ServiceInitialize().Forget();
        }

        public void LateDispose()
        {
            RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteSettings;
            AuthenticationService.Instance.SignOut();
        }

        public void OnRemoteFetchRequest(RemoteConfigFetchRequestSignal signal)
        {
            requestedVariables[signal.key] = signal.type;
            RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes()).AsUniTask().Forget();
        }

        private async UniTaskVoid ServiceInitialize()
        {
#if !PRODUCTION 
            RemoteConfigService.Instance.SetEnvironmentID("75a4b5bb-0e43-41d1-9be4-22a85ae81411");
#endif

            if (!Utilities.CheckForInternetConnection())
                return;

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        }

        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
#if DEVELOPMENT
                    Debug.Log ("No settings loaded this session and no local cache file exists; using default values.");
#endif
                    break;
                case ConfigOrigin.Cached:
#if DEVELOPMENT
                    Debug.Log ("No settings loaded this session; using cached values from a previous session.");
#endif
                    break;
                case ConfigOrigin.Remote:
#if DEVELOPMENT
                    Debug.Log ("New settings loaded this session; update values accordingly.");
#endif
                    break;
            }

            foreach (var pair in requestedVariables)
            {
                switch (pair.Value)
                {
                    case 0:
                        string strValue = RemoteConfigService.Instance.appConfig.GetString(pair.Key);
#if DEVELOPMENT
                        Debug.Log($"Received remote config {pair.Key}: {strValue}");
#endif
                        signalBus.Fire(new RemoteConfigResponseSignal<string>(pair.Key, strValue));
                        break;
                    case 1:
                        int intValue = RemoteConfigService.Instance.appConfig.GetInt(pair.Key);
#if DEVELOPMENT
                        Debug.Log($"Received remote config {pair.Key}");
#endif
                        signalBus.Fire(new RemoteConfigResponseSignal<int>(pair.Key, intValue));
                        break;
                    case 2:
                        //NOTE: need to do float signal
                        break;
                    case 3:
                        //NOTE: need to do boolean signal
                        break;
                }
            }
        }
    }
}