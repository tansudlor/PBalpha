using com.playbux.networking.mirror.message;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.firebaseservice
{
    public readonly struct ClientSettingMessage : NetworkMessage
    {

        public readonly string Path;
        public readonly bool Success;
        public readonly string Result;
        public ClientSettingMessage(string path, string result, bool success)
        {
            Path = path;
            Result = result;
            Success = success;
        }
    }

    public partial class ClientSettingServiceNetwork
    {

        private static ClientSettingServiceNetwork instance;

        public bool ready = false;

        private ClientSettingServiceNetwork()
        {
            ready = false;

#if SERVER
            
            ServerSubscription();
#else
            ClientSubscription();
#endif
        }


        public static void CreateInstance()
        {

            GetInstance();

        }

        public static ClientSettingServiceNetwork GetInstance()
        {

            if (instance == null)
            {
                instance = new ClientSettingServiceNetwork();
            }

            return instance;

        }



    }
}
