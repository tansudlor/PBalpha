#if !SERVER
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
        private SignalBus signalBus;
        private ChangeThisPlayerNameSignal changeThisPlayerNameSignal;
        private IIdentitySystem identitySystem;
        private INetworkMessageReceiver<UserProfileMessage> userProfileMessageReceiver;

        public IdentityDataSystem(INetworkMessageReceiver<UserProfileMessage> userProfilemessageReceiver, SignalBus signalBus, IIdentitySystem identitySystem)
        {
            this.signalBus = signalBus;
            this.identitySystem = identitySystem;
            this.userProfileMessageReceiver = userProfilemessageReceiver;
            this.userProfileMessageReceiver.OnEventCalled += OnUserProfileResponse;
        }

        private void OnUserProfileResponse(UserProfileMessage message)
        {
#if !BINARY_NETWORK
            var command = message.Message.Split(',')[0];
            if (command == "rename")
            {
                string renamePlayer = message.Message.Split(',')[1];
                identitySystem[message.NetId].DisplayName = renamePlayer;
               
                if (NetworkClient.localPlayer.netId == message.NetId)
                {
                    changeThisPlayerNameSignal = new ChangeThisPlayerNameSignal();
                    changeThisPlayerNameSignal.ThisNameChange = renamePlayer;
                    changeThisPlayerNameSignal.NetId = message.NetId;
                    signalBus.Fire(changeThisPlayerNameSignal);
                }

                IdentityDetail clientId = identitySystem[message.NetId];
                clientId.Observer.OnUpdateProfile(clientId);
                return;

            }

#else
            if (message.Cmd == UserProfileCommand.Rename)
            {
                string name = message.ReDataParameter.Name;
                identitySystem[message.NetId].DisplayName = name;

                if (NetworkClient.localPlayer.netId == message.NetId)
                {
                    changeThisPlayerNameSignal = new ChangeThisPlayerNameSignal();
                    changeThisPlayerNameSignal.ThisNameChange = name;
                    changeThisPlayerNameSignal.NetId = message.NetId;
                    signalBus.Fire(changeThisPlayerNameSignal);
                }

                IdentityDetail clientId = identitySystem[message.NetId];
                clientId.Observer.OnUpdateProfile(clientId);
                return;
            }
#endif

        }

        public void OnIdentityChangeSinal(IdentityChangeSignal identityChangeSignal)
        {
            string command = identityChangeSignal.Command;

            if (command == "name")
            {
                string name = (string)identityChangeSignal.Data;
#if !BINARY_NETWORK
            NetworkClient.Send(new UserProfileMessage(NetworkClient.localPlayer.netId, "changename," + name));
#else
                NetworkClient.Send(new UserProfileMessage(NetworkClient.localPlayer.netId, UserProfileCommand.ChangeName, new ReDataParameter(name, PlayerPrefs.GetString(TokenUtility.accessTokenKey))));
#endif
            }

            if(command == "accesstoken")
            {
                string accessToken = (string)identityChangeSignal.Data;
                Debug.Log(accessToken + " Send To Server");
                NetworkClient.Send(new UserProfileMessage(NetworkClient.localPlayer.netId, UserProfileCommand.ChageAccessToken, new ReDataParameter("", accessToken)));
            }



        }

        
    }
}
#endif