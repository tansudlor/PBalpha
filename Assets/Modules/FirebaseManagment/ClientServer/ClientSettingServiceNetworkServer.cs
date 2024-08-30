#if SERVER
using com.playbux.networking.mirror.message;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.firebaseservice
{

    public partial class ClientSettingServiceNetwork
    {
        private IServerNetworkMessageReceiver<ClientSettingMessage> serverMessageReceiver;
        private void ServerSubscription()
        {
            FirebaseAuthenticationService.GetInstance().OnDataComplete += OnDataComplete;
            //serverMessageReceiver.OnEventCalled += OnServerMessageReceive;
            NetworkServer.RegisterHandler<ClientSettingMessage>(OnServerMessageReceive);
        }

        private void OnServerMessageReceive(NetworkConnectionToClient connection, ClientSettingMessage message, int channel)
        {
            Debug.Log("callthiswhenclientCall");
            //user call dialog/feature
            Debug.Log(message.Path + " message.Path");
            var accessiblePath = "/";
            JObject settingData = GetSettingData();
            Debug.Log(settingData.ToString());
            var path = message.Path;
            string[] childs = path.Split('/');
            bool accessible = true;
            JToken currentlevel = settingData;
            Debug.Log(currentlevel.ToString());
            Debug.Log(currentlevel["dialog"].ToString());
            Debug.Log(currentlevel["dialog"]["shop_to_earn"].ToString());
            try
            {
                for (int i = 0; i < childs.Length; i++)
                {

                    currentlevel = currentlevel[childs[i]];
                    accessiblePath += childs[i] + "/";
                    Debug.Log(currentlevel.ToString() + " : " + childs[i]);
                }
            }
            catch
            {
                accessible = false;
            }

            if (accessiblePath.Length > 0)
            {
                accessiblePath = accessiblePath.Remove(accessiblePath.Length - 1);
            }
            string settingDataToSend = JsonConvert.SerializeObject(currentlevel);
            Debug.Log(accessiblePath + "accessiblePath");
            Debug.Log(settingDataToSend + "settingDataToSend");
            Debug.Log(accessible + "accessible");
            connection.Send(new ClientSettingMessage(accessiblePath, settingDataToSend, accessible));
        }


        public JObject GetSettingData()
        {


            if (!ready)
            {
                return null;
            }
            
            return FirebaseAuthenticationService.GetInstance().ClientSettingData;
        }


        private void OnDataComplete(JObject settingData)
        {
            ready = true;
        }


        public string ServiceReport()
        {
            if (!ready)
            {
                return "Service not ready";
            }

            return JsonConvert.SerializeObject(GetSettingData());
        }


    }
}
#endif
