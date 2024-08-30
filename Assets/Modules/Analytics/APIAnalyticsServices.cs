using com.playbux.api;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.analytic
{
    public class APIAnalyticsServices : IAnalyticServices
    {
        public APIAnalyticsServices()
        {

        }

        public void Log(string eventName, params LogParameter[] param)
        {

            RestAPIAnalyticsData restAPIAnalyticsData = new RestAPIAnalyticsData(eventName, param);
            string jsonData = JsonConvert.SerializeObject(restAPIAnalyticsData);
            Debug.Log(jsonData);
            //FirebaseAnalytics.LogEvent(eventName, firebaseParam);
            APIServerConnector.AnalyticsAPI(jsonData).Forget();
            Debug.Log("set");
        }
    }

    public class RestAPIAnalyticsData
    {
        private string eventName;
        private LogParameter[] parameters;
        [JsonProperty("event_name")]
        public string EventName { get => eventName; set => eventName = value; }
        [JsonProperty("parameters")]
        public LogParameter[] Parameters { get => parameters; set => parameters = value; }
        public RestAPIAnalyticsData(string eventName, LogParameter[] parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }


    }

}
