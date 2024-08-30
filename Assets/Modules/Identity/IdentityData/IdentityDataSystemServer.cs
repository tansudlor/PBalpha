#if SERVER
using com.playbux.api;
using com.playbux.events;
using com.playbux.networking.mirror.message;
using Cysharp.Threading.Tasks;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace com.playbux.identity
{
    public partial class IdentityDataSystem
    {
        private IIdentitySystem identitySystem;
        private IServerNetworkMessageReceiver<UserProfileMessage> userProfilemessageReceiver;

        public IdentityDataSystem(SignalBus signalBus, IServerNetworkMessageReceiver<UserProfileMessage> userProfilemessageReceiver, IIdentitySystem identitySystem)
        {
#if DEVELOPMENT
            Debug.Log("IDM sub");
#endif
            this.identitySystem = identitySystem;
            this.userProfilemessageReceiver = userProfilemessageReceiver;
            this.userProfilemessageReceiver.OnEventCalled += OnUserDataRequire;
        }

        private void OnUserDataRequire(NetworkConnectionToClient connection, UserProfileMessage message, int channel)
        {

#if DEVELOPMENT
            Debug.Log("Server IDM received message: " + message.NetId);
            Debug.Log("Server IDM Find NetworkIdentity");
            Debug.Log("Require IDM *************************" + connection);
#endif
#if !BINARY_NETWORK
            var command = message.Message.Split(',')[0];
            Debug.Log("id " + message.NetId);
            if (command == "changename")
            {

                var name = message.Message.Split(',')[1];
                DisplayName displayName = new DisplayName();
                displayName.display_name = name;
                Debug.Log(name);
                identitySystem[message.NetId].DisplayName = name;
                NetworkServer.SendToReadyObservers(NetworkServer.spawned[message.NetId], new UserProfileMessage(message.NetId, "rename," + name)); //rename localplayer to observer player
                APIServerConnector.UpdateName(identitySystem[message.NetId].UID, JsonConvert.SerializeObject(displayName)).Forget();
            }
#else
            
            if (message.Cmd == UserProfileCommand.ChangeName)
            {
                string name = message.ReDataParameter.Name;
                string accessToken = message.ReDataParameter.AccessToken;   
                DisplayName displayName = new DisplayName();
                displayName.display_name = name;
                identitySystem[message.NetId].DisplayName = name;
                NetworkServer.SendToReadyObservers(NetworkServer.spawned[message.NetId], new UserProfileMessage(message.NetId, UserProfileCommand.Rename, new ReDataParameter(name,"")));
                APIServerConnector.UpdateName(accessToken, JsonConvert.SerializeObject(displayName)).Forget();
            }

            if(message.Cmd == UserProfileCommand.ChageAccessToken)
            {
                identitySystem[message.NetId].AccessToken = message.ReDataParameter.AccessToken;

            }


#endif

        }
    }

    public class DisplayName
    {
        public string display_name { get; set; }

       
    }

}
#endif