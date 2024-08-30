#if !BINARY_NETWORK
using Mirror;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace com.playbux.networking.mirror.message
{


    public readonly struct UserProfileMessage : NetworkMessage
    {

        public readonly uint NetId;
        public readonly string Message;
        public UserProfileMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
        }

    }

    public readonly struct UserDataMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Message;
        public UserDataMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
        }
    }
}
#endif