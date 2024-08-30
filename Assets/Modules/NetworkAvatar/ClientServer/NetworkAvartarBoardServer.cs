#if SERVER
using com.playbux.avatar;
using Mirror;
using UnityEngine;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.message;
using com.playbux.api;
using com.playbux.identity;

namespace com.playbux.networking.networkavatar
{
    public partial class NetworkAvatarBoard
    {
        private IIdentitySystem identitySystem;

        public NetworkAvatarBoard(IServerNetworkMessageReceiver<AvatarUpdateMessage> messageReceiver, IIdentitySystem identitySystem, AvatarSampleSet avatarSampleSet)
        {
            messageReceiver.OnEventCalled += OnAvatarRequire;
            defaultAvatarSet = new AvatarSet();
            this.avatarSampleSet = avatarSampleSet;
            this.identitySystem = identitySystem;
            DelayAvatarPayload().Forget();
            Log("NAB sub");
        }

        private async void OnAvatarRequire(NetworkConnectionToClient connection, AvatarUpdateMessage message, int channel)
        {
            Log("Server received message: " + message.NetId);
            Log("Server Find NetworkIdentity");
            Log("Require*************************" + connection);
            if (message.Message == "")
            {
                GetAvatarSet(message.NetId, connection);
            }
            else
            {
                avatars[message.NetId] = new AvatarSet(message.Message);
                Debug.Log(avatars[message.NetId].JSONForAPI());
                avatars[message.NetId].Ticker = 10;
                avatars[message.NetId].Payload = avatars[message.NetId].JSONForAPI();

                NetworkServer.SendToReadyObservers(connection.identity, new AvatarUpdateMessage(message.NetId, message.Message));
            }

            //NetworkServer.SendToReadyObservers(connection.identity, new AvatarUpdateMessage(message.NetId, ""));
        }


        private async UniTask DelayAvatarPayload()
        {
            while (true)
            {
                await UniTask.Delay(1000);
                foreach (var key in avatars.Keys)
                {
                    if (!string.IsNullOrEmpty(avatars[key].Payload))
                    {
                        avatars[key].Ticker -= 1;
                        if (avatars[key].Ticker < 0)
                        {
                            APIServerConnector.UpdateAvatar(identitySystem[key].AccessToken, avatars[key].Payload).Forget();
                            avatars[key].Payload = null;
                            avatars[key].Ticker = 300;

                        }

                    }
                }
            }
        }

        private IAvatarSet GetAvatarSetServer(uint playerId, NetworkConnectionToClient connection)
        {
            if (!avatars.ContainsKey(playerId)) // this is not gonna happen
            {
                // tell GameSevices API  for sync avatars
                Log("API*************************" + connection);
                FAKEAPI(playerId, connection).Forget();
                //default avatar GameSevices API response
                NetworkServer.SendToReadyObservers(connection.identity, new AvatarUpdateMessage(playerId, avatarSampleSet.slotData[0]));
                return defaultAvatarSet;
            }
            else
            {
                NetworkServer.SendToReadyObservers(connection.identity, new AvatarUpdateMessage(playerId, avatars[playerId].ToJSON()));
                return avatars[playerId];
            }
        }

        private async UniTaskVoid FAKEAPI(uint playerId, NetworkConnectionToClient connection)
        {
            Log("Server Download Avatar");

            await UniTask.Delay(3000);
            string fakejson = avatarSampleSet.slotData[Random.Range(1, avatarSampleSet.slotData.Length)];
            Log("Server Write Avatar to Board");
            avatars[playerId] = new AvatarSet(fakejson);
            Log("Server update Avatar to Client Board");
            NetworkServer.SendToReadyObservers(connection.identity, new AvatarUpdateMessage(playerId, fakejson));
            // UpdateAvatar(playerId, new AvatarSet(fakejson));
        }
    }
}
#endif