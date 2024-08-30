#if !SERVER
using com.playbux.avatar;
using Mirror;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.message;
using UnityEngine;


namespace com.playbux.networking.networkavatar
{
    public partial class NetworkAvatarBoard
    {
        public NetworkAvatarBoard(INetworkMessageReceiver<AvatarUpdateMessage> messageReceiver, AvatarSampleSet avatarSampleSet)
        {
            messageReceiver.OnEventCalled += OnAvatarResponse;
            defaultAvatarSet = new AvatarSet();
            this.avatarSampleSet = avatarSampleSet;
            //       SendRequireToServer(NetworkClient.connection.identity).Forget();
        }

        private IAvatarSet GetAvatarSetClient(uint playerId, bool updateSet)
        {
            if (!avatars.ContainsKey(playerId))
            {
                // tell server for sync avatars
                SendRequireToServer(playerId).Forget();
                //default avatar wait server response
                return defaultAvatarSet;
            }
            else
            {
                if (updateSet)
                    UpdateAvatar(playerId, avatars[playerId]);
                return avatars[playerId];
            }
        }

        public override void ChangePart(uint playerId, EquipInfo assign)
        {
            base.ChangePart(playerId, assign);
            Log("______Send");
            NetworkClient.Send(new AvatarUpdateMessage(playerId, avatars[playerId].ToJSON()));
        }

        private void OnAvatarResponse(AvatarUpdateMessage message)
        {
            Debug.Log(message.Message);
            if (message.Message != "")
            {
                Log("Server received message: " + message.NetId);
            }
            //NetworkClient.Send(new AvatarUpdateMessage(message.NetId + 1));
            Log("______Recv");
            UpdateAvatar(message.NetId, new AvatarSet(message.Message));
        }

        private async UniTaskVoid SendRequireToServer(uint playerId)
        {
            await UniTask.Delay(100);
            NetworkClient.Send(new AvatarUpdateMessage(playerId, ""));
        }
    }
}
#endif
