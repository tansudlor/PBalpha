#if !SERVER
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using com.playbux.networking.mirror.message;
using Zenject;
using System.Reflection;
using Cysharp.Threading.Tasks;
using com.playbux.events;
using com.playbux.api;

namespace com.playbux.identity
{
    public partial class IdentitySystem : ICredentialProvider, IInitializable, ILateDisposable
    {
        private readonly INetworkMessageReceiver<PlayerListMessage> listMessageReceiver;
        private readonly INetworkMessageReceiver<UserDataMessage> dataMessageReceiver;
        private readonly INetworkMessageReceiver<UserStatusMessage> dataUserStatusMessageReceiver;
        private SignalBus signalBus;


        public IdentitySystem(INetworkMessageReceiver<PlayerListMessage> listMessageReceiver,
            INetworkMessageReceiver<UserDataMessage> dataMessageReceiver, INetworkMessageReceiver<UserStatusMessage> dataUserStatusMessageReceiver,
            SignalBus signalBus)
        {
            identity = new Dictionary<uint, IdentityDetail>();
            NameReverse = new Dictionary<string, uint>();
            this.listMessageReceiver = listMessageReceiver;
            this.dataMessageReceiver = dataMessageReceiver;
            this.dataMessageReceiver.OnEventCalled += OnNetworkResponse;
            this.signalBus = signalBus;
            this.dataUserStatusMessageReceiver = dataUserStatusMessageReceiver;
            this.dataUserStatusMessageReceiver.OnEventCalled += OnStatusRequest;
        }


        private void OnNetworkResponse(UserDataMessage message)
        {
#if DEVELOPMENT
#if !BINARY_NETWORK
            Debug.Log("IDN: " + message.Message);
#endif
#endif
#if !BINARY_NETWORK
           
            var command = message.Message.Split(',')[0];
            if (command == "userdata")
            {
                string userData = message.Message[(message.Message.IndexOf(',') + 1)..];
                IdentityDetail serverId = JsonConvert.DeserializeObject<IdentityDetail>(userData);
                IdentityDetail clientId = this[message.NetId];

                foreach (PropertyInfo item in serverId.GetType().GetProperties())
                {
                    if (item.GetValue(serverId) == null)
                    {
                        continue;
                    }

                    clientId.GetType().GetProperty(item.Name).SetValue(clientId, item.GetValue(serverId));
                }

                clientId.Observer.OnUpdateProfile(clientId);
                return;
            }
#else
            if (message.Cmd == UserDataCommand.UserData)
            {
                IdentityDetail clientId = this[message.NetId];
                Debug.Log(JsonConvert.SerializeObject(message.UserDataParameter) + " clientIDUserData");
                message.UserDataParameter.ApplyPropertiesObject(clientId);
                Debug.Log(JsonConvert.SerializeObject(clientId) + " clientID");
                clientId.Observer.OnUpdateProfile(clientId);
                return;
            }

            if (message.Cmd == UserDataCommand.UserBalance)
            {
                int brk = message.UserDataParameter.BalanceBrk;
                int lotto = message.UserDataParameter.BalanceLottoTickets;
                int pebble = message.UserDataParameter.BalancePebble;

                UserBalance balance = new UserBalance();
                balance.Brk = brk;
                balance.Lotto = lotto;
                balance.Pebble = pebble;

                QuestRewardSignal questRewardSignal = new QuestRewardSignal();
                questRewardSignal.Data = balance;
                signalBus.Fire(questRewardSignal);

            }

#endif
        }

        public void GetUserdata(NetworkIdentity identity)
        {
#if !BINARY_NETWORK
            NetworkClient.Send(new UserDataMessage(identity.netId, "me"));
#else
            UserDataParameter userDataParameter = new UserDataParameter();
            NetworkClient.Send(new UserDataMessage(identity.netId, UserDataCommand.Me, userDataParameter));
#endif
        }

        async UniTask WaithForObserver(IdentityDetail clientId)
        {
            await UniTask.WaitUntil(() => clientId.Observer != null);
            clientId.Observer.OnUpdateProfile(clientId);
        }


        public NetworkIdentity GetData(string name)
        {
            if (!NameReverse.ContainsKey(name))
                return null;

            if (this[NameReverse[name]] == null)
                return null;

            return this[NameReverse[name]].Identity;
        }

        public string GetData(NetworkIdentity identity)
        {

            return this[identity.netId].UID;
        }

        public void Initialize()
        {
            listMessageReceiver.OnEventCalled += OnServerUpdated;
        }

        public void LateDispose()
        {
            listMessageReceiver.OnEventCalled -= OnServerUpdated;
        }

        public void OnPlayerAuthenticated(string name, uint netId)
        {
        }

        public void OnPlayerDisconnected(string name)
        {
        }

        public void Update()
        {
        }

        private void OnServerUpdated(PlayerListMessage message)
        {
            nameReverse.Clear();

            for (int i = 0; i < message.Names.Length; i++)
            {
                nameReverse.TryAdd(message.Names[i], message.NetIds[i]);
            }
        }

        private void OnStatusRequest(UserStatusMessage message)
        {
            if (message.Message == "ping")
            {
                Debug.Log(message.Message); //new player login to game when no netID
                try
                {
                    NetworkClient.Send(new UserStatusMessage(NetworkClient.localPlayer.netId, "pong"));
                }
                catch
                {

                }
                
                return;
            }
            else if(message.Message == "killtheoldnetid")
            {
                signalBus.Fire(new NotificationUISignal(1, "Your account has been logged in \r\non other devices right now. \r\nPlease update your password."));
            }
        }
    }
}
#endif