using System;
using Zenject;
using UnityEngine;
using com.playbux.api;
using System.Text.Json;
using com.playbux.events;
using Cysharp.Threading.Tasks;
using com.playbux.utilis.network;
using System.Collections.Specialized;

namespace com.playbux.network.api.rest
{
    [Serializable]
    public class CTECountryStat
    {
        public int rank;
        public int country;
        public int total_pixel_conquer;
        public CTECountryData country_data;
    }

    [Serializable]
    public class CTECountryData
    {
        public string code;
        public string color;
        public string country;
    }

    public class RESTDataStore : IDataStore, IInitializable, ILateTickable
    {
        public DomainEnum DomainEnum => domainEnum;

        private readonly IRESTWorker worker;
        private readonly SignalBus signalBus;
        private readonly UrlHandler urlHandler;
        private readonly DomainEnum domainEnum;
        private readonly APISettings apiSettings;
        private readonly IRequestHandler<object> request;

        private bool isReady;


        public RESTDataStore(IRESTWorker worker, IRequestHandler<object> request, SignalBus signalBus, UrlHandler urlHandler, APISettings apiSettings)
        {
            this.worker = worker;
            this.request = request;
            this.signalBus = signalBus;
            this.urlHandler = urlHandler;
            this.apiSettings = apiSettings;
        }

        public void Initialize()
        {
            RequestDomain().Forget();
        }

        public void LateTick()
        {
            if (!isReady)
                return;

#if !SERVER
            ConquerToEarnCallLoop();
#endif
        }

        #region Domain Remote Config
        public void OnRemoteConfigDomainResponse(RemoteConfigResponseSignal<string> signal)
        {
            if (apiSettings.domain.Key != signal.key)
                return;

#if DEBUG
            Debug.Log($"Url received {signal.value}");
#endif

            apiSettings.domain.Value = signal.value;
            isReady = !string.IsNullOrEmpty(apiSettings.domain.Value) && !string.IsNullOrEmpty(apiSettings.apiVersion.Value);
        }

        public void OnRemoteConfigApiVersionResponse(RemoteConfigResponseSignal<string> signal)
        {
            if (apiSettings.apiVersion.Key != signal.key)
                return;

#if DEBUG
            Debug.Log($"Version received {signal.value}");
#endif

            apiSettings.apiVersion.Value = signal.value;
            isReady = !string.IsNullOrEmpty(apiSettings.domain.Value) && !string.IsNullOrEmpty(apiSettings.apiVersion.Value);
        }

        private async UniTaskVoid RequestDomain()
        {
            await UniTask.WaitForSeconds(3f);
            signalBus.Fire(new RemoteConfigFetchRequestSignal(apiSettings.domain.Key, (int)apiSettings.domain.Type));
            signalBus.Fire(new RemoteConfigFetchRequestSignal(apiSettings.apiVersion.Key, (int)apiSettings.apiVersion.Type));
        }
        #endregion

        #region Conquer To Earn
#if !SERVER
        private const float CTE_RECALL_TIME = 600f;
        private const string CTE_SEGMENT = "conquer-to-earn/ranking";
        private float currentCteTime = CTE_RECALL_TIME;

        private async UniTaskVoid RequestCteToCache()
        {
            var message = await Get<CTECountryStat[]>(HttpUtility.ParseQueryString(""), CTE_SEGMENT);

#if DEBUG
            string debugStr = "";

            for (int i = 0; i < message.data.Length; i++)
            {
                if (!string.IsNullOrEmpty(message.errorMessage.error))
                {
                    debugStr += message.errorMessage.statusCode + " " + message.errorMessage.message;
                    break;
                }

                debugStr += message.data[i].rank + " " + message.data[i].country_data.country + "\n";
            }

            Debug.Log(debugStr);
#endif
            var dataSignal = new CTETopFiveDataSignal();
            for (int i = 0; i < 5; i++)
            {
                if (message.data == null || message.data.Length < 5)
                    break;

                dataSignal.ranks[i] = message.data[i].rank;
                dataSignal.colors[i] = message.data[i].country_data.color;
                dataSignal.totalPixels[i] = message.data[i].total_pixel_conquer;
                dataSignal.countryNames[i] = message.data[i].country_data.country;
            }

            signalBus.Fire(dataSignal);
        }

        private void ConquerToEarnCallLoop()
        {
            if (currentCteTime >= CTE_RECALL_TIME)
            {
                RequestCteToCache().Forget();
                currentCteTime = 0;
                return;
            }

            currentCteTime += Time.fixedUnscaledDeltaTime;
        }

#endif
        #endregion

        #region REST Method
        public async UniTask<RestMessage<T>> Post<T>(NameValueCollection query, object data, string segments)
        {
            var url = urlHandler.Handle(segments, query);
            var webRequest = request.CreateRequest(url, "POST", data);
            var json = await worker.Handle(webRequest);
            T response;
            bool maybeAnArray = false;
            try
            {
                response = JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                if (e.HResult == -2147024809)
                    maybeAnArray = true;

                response = default;
                Debug.LogWarning(e.HResult);
            }

            var error = JsonUtility.FromJson<RestErrorMessage>(json);
            var entity = maybeAnArray ? JsonUtility.FromJson<RestMessage<T>>(json) : new RestMessage<T>(response, error);
            return entity;
        }

        public async UniTask<RestMessage<T>> Post<T>(object data, string segments)
        {
            var url = urlHandler.Handle(segments);
            var webRequest = request.CreateRequest(url, "POST", data);
            var json = await worker.Handle(webRequest);
            T response;
            bool maybeAnArray = false;
            try
            {
                response = JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                if (e.HResult == -2147024809)
                    maybeAnArray = true;

                response = default;
                Debug.LogWarning(e.HResult);
            }

            var error = JsonUtility.FromJson<RestErrorMessage>(json);
            var entity = maybeAnArray ? JsonUtility.FromJson<RestMessage<T>>(json) : new RestMessage<T>(response, error);
            return entity;
        }

        public async UniTask<RestMessage<T>> Get<T>(NameValueCollection query, string segments)
        {
            var url = urlHandler.Handle(segments, query);
            var webRequest = request.CreateRequest(url, "GET", null);
            var json = await worker.Handle(webRequest);
            T response;
            bool maybeAnArray = false;
            try
            {
                response = JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                if (e.HResult == -2147024809)
                    maybeAnArray = true;

                response = default;
                Debug.LogWarning(e.HResult);
            }

            var error = JsonUtility.FromJson<RestErrorMessage>(json);
            var entity = maybeAnArray ? JsonUtility.FromJson<RestMessage<T>>(json) : new RestMessage<T>(response, error);
            return entity;
        }
        #endregion
    }
}
