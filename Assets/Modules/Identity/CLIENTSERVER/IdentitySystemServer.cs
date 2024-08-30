#if SERVER
using Mirror;
using System.Collections.Generic;
using Newtonsoft.Json;
using com.playbux.api;
using System.Linq;
using com.playbux.networking.mirror.message;
using Cysharp.Threading.Tasks;
using UnityEngine;
using com.playbux.events;
using Zenject;
using System;

namespace com.playbux.identity
{
    public partial class IdentitySystem : ICredentialProvider, IIdentitySystem
    {
        private IServerNetworkMessageReceiver<UserDataMessage> dataMessageReceiver;
        private IServerNetworkMessageReceiver<UserStatusMessage> dataUserStatusMessageReceiver;
        private SignalBus signalBus;
        private Dictionary<NetworkConnectionToClient, string> OnlinePlayer = new Dictionary<NetworkConnectionToClient, string>();

        public IdentitySystem(IServerNetworkMessageReceiver<UserDataMessage> dataMessageReceiver, SignalBus signalBus, IServerNetworkMessageReceiver<UserStatusMessage> dataUserStatusMessageReceiver)
        {
            this.signalBus = signalBus;
            identity = new Dictionary<uint, IdentityDetail>();
            NameReverse = new Dictionary<string, uint>();
            dataMessageReceiver.OnEventCalled += OnNetworkRequest;


            this.signalBus.Subscribe<QuestRewardSignal>(OnRewardUpdate);
            this.signalBus.Subscribe<RemoteConfigResponseSignal<string>>(OnRemoteConfigResponseSignal);

#if PRODUCTION
            this.signalBus.Fire(new RemoteConfigFetchRequestSignal("api-key", 0));
#endif
            this.dataUserStatusMessageReceiver = dataUserStatusMessageReceiver;
            this.dataUserStatusMessageReceiver.OnEventCalled += OnStatusRequest;
            CheckConnectedPlayer();
        }

        public void OnRemoteConfigResponseSignal(RemoteConfigResponseSignal<string> signal)
        {
#if PRODUCTION
            if (signal.key == "api-key")
            {
                APIServerConnector.apiKey = signal.value;
            }
#endif
        }

        public void ProfileUserData(UserProfile userData, uint netId)
        {
            this[netId].ID = userData._id;
            this[netId].UID = userData.uid;
            this[netId].NetId = netId;
            this[netId].LoginTime = DateTime.UtcNow;
            this[netId].AccessToken = userData.accressToken;
            this[netId].DisplayName = userData.display_name;
            this[netId].Equipments = userData.equipments;
            this[netId].Identity = NetworkServer.spawned[netId];
            this[netId].BalanceBrk = userData.wallet != null ? userData.wallet.brk.amount_unsafe : 0;
            this[netId].BalanceLottoTickets = userData.wallet != null ? userData.wallet.lotto_ticket.amount_unsafe : 0;
            this[netId].Wallet = userData.wallet;
            this[netId].BalancePebble = userData.wallet != null ? userData.wallet.pebble.amount_unsafe : 0;
            this[netId].CanPlayQuiz = userData.canPlayQuiz;
            this[netId].Email = userData.email;
            nameReverse[this[netId].UID] = netId;
            IdentityDetail detail = identity[netId];
            var detailText = JsonConvert.SerializeObject(detail);
            NetworkServer.SendToReady(new PlayerListMessage(nameReverse.Keys.ToArray(), nameReverse.Values.ToArray()));
        }

        private void OnNetworkRequest(NetworkConnectionToClient connection, UserDataMessage message, int channel)
        {
#if !BINARY_NETWORK
            var command = message.Message;
            if (command == "me")
            {
                IdentityDetail detail = identity[message.NetId];
                var detailText = JsonConvert.SerializeObject(detail);
                connection.Send(new UserDataMessage(message.NetId, "userdata," + detailText));
                return;

            }

#else
            if (message.Cmd == UserDataCommand.Me)
            {
                IdentityDetail detail = identity[message.NetId];
                Debug.Log("loginTime" + detail.LoginTime);
                connection.Send(new UserDataMessage(message.NetId, UserDataCommand.UserData, new UserDataParameter(detail.ID, detail.UID, detail.UserName, detail.NetId, detail.DisplayName, detail.Equipments, detail.BalanceBrk, detail.BalanceLottoTickets, detail.LoginTime, detail.BalancePebble,detail.Email)));
                return;
            }


#endif
        }

        public void Update()
        {
            NetworkServer.SendToReady(new PlayerListMessage(NameReverse.Keys.ToArray(), NameReverse.Values.ToArray()));
        }

        public void OnRewardUpdate(QuestRewardSignal signal)
        {
            if (signal.Command == "questrewardupdate")
            {
                //Debug.Log(signal.Command);
                UserBalance userBalance = (UserBalance)signal.Data;

                int brk = userBalance.Brk;
                int lotto = userBalance.Lotto;
                int pebble = userBalance.Pebble;
                var netID = signal.NetId;

                UserDataMessage userDataMessage = new UserDataMessage(netID, UserDataCommand.UserBalance, new UserDataParameter("", "", "", 0, "", new Equipments(), brk, lotto, new DateTime(0), pebble,""));
                //Debug.Log(JsonConvert.SerializeObject(new UserDataParameter("", "", "", 0, "", new Equipments(), brk, lotto, new DateTime(0), pebble)));
                NetworkServer.spawned[netID].connectionToClient.Send(userDataMessage);
            }

        }


        public NetworkIdentity GetData(string uid)
        {
            if (!NameReverse.ContainsKey(uid))
                return null;

            return this[NameReverse[uid]].Identity;
        }

        public string GetData(NetworkIdentity networkIdentity)
        {
            return identity[networkIdentity.netId].UID;
        }

        public void OnPlayerAuthenticated(string uid, uint netId)
        {
            UserProfile userProfile = APIServerConnector.userProfileCache[uid];

            if (NameReverse.ContainsKey(userProfile.uid))
            {
                var oldNetId = NameReverse[userProfile.uid];
                //this[oldNetId].Identity.connectionToClient.Disconnect();
                this[oldNetId].Identity.connectionToClient.Send(new UserStatusMessage(0, "killtheoldnetid"));
                NameReverse.Remove(userProfile.uid);
                
            }

#if SERVER
            ProfileUserData(userProfile, netId);
#endif
        }

        public void OnPlayerDisconnected(string uid)
        {
            var details = identity.Where(i => i.Value.UID == uid);

            if(details.Count() <= 0)
            {
                return;
            }

            NameReverse.Remove(uid);
            foreach (var item in details)
            {
                uint netId = item.Key;
                if (identity.ContainsKey(netId))
                {
                    identity.Remove(netId);
                }
            }
        }

        private void OnStatusRequest(NetworkConnectionToClient connection, UserStatusMessage message, int channel)
        {
            if (message.Message == "pong")
            {
                Debug.Log(message.Message);
                OnlinePlayer[connection] = "1";
                return;
            }
        }

        public async void CheckConnectedPlayer()
        {
            while (true)
            {
                foreach (var connection in NetworkServer.connections)
                {
                    if (OnlinePlayer.ContainsKey(connection.Value))
                    {
                        if (OnlinePlayer[connection.Value] == "0")
                        {
                            connection.Value.Disconnect();
                            OnlinePlayer.Remove(connection.Value);
                        }
                    }

                    OnlinePlayer[connection.Value] = "0";

                    connection.Value.Send(new UserStatusMessage(0, "ping"));

                }

                await UniTask.WaitForSeconds(300f);
            }
        }

    }
}
#endif