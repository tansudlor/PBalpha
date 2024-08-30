using Zenject;
using com.playbux.events;
using UnityEngine.Networking;
using com.playbux.utilis.network;
using UnityEngine;

namespace com.playbux.network.api.rest
{
    public interface IRequestHandler<T>
    {
        UnityWebRequest CreateRequest(string url, string method, T data);
    }
    public class JsonRequest : IRequestHandler<object>, IInitializable, ILateDisposable
    {
        private string API_KEY_CONFIG_KEY = "api-key";

        private readonly SignalBus signalBus;
        private readonly APICredential apiCredential;

        private string apiKey;

        public JsonRequest(SignalBus signalBus, APICredential apiCredential)
        {
            this.signalBus = signalBus;
            this.apiCredential = apiCredential;
            this.signalBus.Subscribe<RemoteConfigResponseSignal<string>>(OnRESTSecretKeyConfigResponse);
        }

        public void Initialize()
        {
            signalBus.Fire(new RemoteConfigFetchRequestSignal(API_KEY_CONFIG_KEY, (int)RemoteConfigType.String));

            
        }

        public void LateDispose()
        {
            signalBus.Unsubscribe<RemoteConfigResponseSignal<string>>(OnRESTSecretKeyConfigResponse);
        }

        public UnityWebRequest CreateRequest(string url, string method, object data = null)
        {
            UnityWebRequest req = new UnityWebRequest();
            Debug.Log("secretKey " + apiKey);
#if SERVER
            if (!string.IsNullOrEmpty(apiKey))
                req.SetRequestHeader("x-api-key", apiKey);
#endif

            if (!string.IsNullOrEmpty(apiCredential.AccessToken))
                req.SetRequestHeader("authorization", "Bearer " + apiCredential.AccessToken);

            if (data != null)
                req.uploadHandler = HttpUtility.HandleData(data);

           

            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 5;
            req.downloadHandler = new DownloadHandlerBuffer();
            req.url = url;
            req.method = method;
            return req;
        }

        private void OnRESTSecretKeyConfigResponse(RemoteConfigResponseSignal<string> signal)
        {
/*#if SERVER && UNITY_EDITOR
            return;
#endif*/
            if (signal.key != API_KEY_CONFIG_KEY)
                return;

            apiKey = signal.value;
            
        }
    }

    public class FormRequest : IRequestHandler<string>
    {
        private readonly APICredential apiCredential;

        public FormRequest(APICredential apiCredential)
        {
            this.apiCredential = apiCredential;
        }

        public UnityWebRequest CreateRequest(string url, string method, string data)
        {
            UnityWebRequest req = new UnityWebRequest();

            if (!string.IsNullOrEmpty(apiCredential.AccessToken))
                req.SetRequestHeader("authorization", "Bearer " + apiCredential.AccessToken);

            req.uploadHandler = new UploadHandlerFile(data);
            req.timeout = 15;
            req.downloadHandler = new DownloadHandlerBuffer();
            req.url = url;
            req.method = method;
            return req;
        }
    }

}