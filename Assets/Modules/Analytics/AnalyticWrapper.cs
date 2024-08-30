using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace com.playbux.analytic
{
    public interface IAnalyticServices
    {
        void Log(string eventName, params LogParameter[] param);
    }

    public class LogParameter
    {
        private string key;
        private string value;
        [JsonProperty("key")]
        public string Key { get => key; set => key = value; }
        [JsonProperty("value")]
        public string Value { get => value; set => this.value = value; }
        public LogParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }


    }

    public class AnalyticWrapper
    {
        private static AnalyticWrapper instance;
        private Dictionary<string, IAnalyticServices> analyticsServices;
        public string DefaultAnalyticsServices;

#if UNITY_STANDALONE_WIN
        public string Platform = "Windows";
#else
        public string Platform = "MacOS";
#endif

        private AnalyticWrapper()
        {
            analyticsServices = new Dictionary<string, IAnalyticServices>();
            DefaultAnalyticsServices = "API";
            AddAnalyticsServices(DefaultAnalyticsServices, new APIAnalyticsServices());
        }

        public void AddAnalyticsServices(string serviceName, IAnalyticServices analytic)
        {
            analyticsServices[serviceName] = analytic;
        }

        public static AnalyticWrapper getInstance()
        {
            if (instance == null)
            {
                instance = new AnalyticWrapper();
                return instance;
            }
            return instance;

        }

        public void Log(string services, string eventname, params LogParameter[] param) //From other service
        {
            try
            {
                var modifyParam = param.ToList();
                modifyParam.Add(new LogParameter("platform", Platform));
                param = modifyParam.ToArray();
                analyticsServices[services].Log("unt_" + eventname, param);
            }
            catch (Exception e)
            {
                // analyticsServices[DefaultAnalyticsServices].Log("game_track_fail", param);
            }
        }

        public void Log(string eventname, params LogParameter[] param)//DefaultService
        {
            var modifyParam = param.ToList();
            modifyParam.Add(new LogParameter("platform", Platform));
            param = modifyParam.ToArray();
            analyticsServices[DefaultAnalyticsServices].Log("unt_" + eventname, param);
        }


    }
}
