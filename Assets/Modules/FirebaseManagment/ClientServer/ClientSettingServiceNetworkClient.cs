#if !SERVER
using com.playbux.networking.mirror.message;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.firebaseservice
{

    public partial class ClientSettingServiceNetwork
    {
        //private INetworkMessageReceiver<ClientSettingMessage> clientMessageReceiver;

        private Action<string, JObject, bool> onSettingDataReceive;

        public Action<string, JObject, bool> OnSettingDataReceive { get => onSettingDataReceive; set => onSettingDataReceive = value; }

        public void GetSettingData(string path)
        {

            NetworkClient.Send(new ClientSettingMessage(path, "", false));

        }

        private void ClientSubscription()
        {
            NetworkClient.RegisterHandler<ClientSettingMessage>(OnClientSettingMessageResponse);
            //clientMessageReceiver.OnEventCalled += OnClientSettingMessageResponse;
        }

        private void OnClientSettingMessageResponse(ClientSettingMessage message)
        {
            string path = message.Path;
            string result = message.Result;
            bool success = message.Success;

            JObject deserializeResult = JsonConvert.DeserializeObject<JObject>(result);
            onSettingDataReceive?.Invoke(path, deserializeResult, success);
        }



    }
}
#endif
