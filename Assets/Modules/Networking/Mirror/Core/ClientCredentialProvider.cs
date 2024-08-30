using System;
using System.Collections.Generic;
using System.Linq;
using com.playbux.networking.mirror.message;
using JetBrains.Annotations;
using Mirror;
using Zenject;

//namespace com.playbux.networking.mirror.core
namespace com.playbux.identity
{
    [Serializable]
    public enum UserClearanceLevel
    {
        Terminated = -2,
        Suspended = -1,
        Normal = 0,
        Moderator,
        Developer,
        System
    }

    public readonly struct UserClearanceData : IEquatable<UserClearanceData>
    {
        public readonly string Title;
        public readonly ushort ClearanceLevel;
        public UserClearanceData(string title, ushort clearanceLevel)
        {
            Title = title;
            ClearanceLevel = clearanceLevel;
        }
        public bool Equals(UserClearanceData other)
        {
            return Title == other.Title && ClearanceLevel == other.ClearanceLevel;
        }
        public override bool Equals(object obj)
        {
            return obj is UserClearanceData other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Title, (int)ClearanceLevel);
        }
    }

    public class UserLevelClearanceProvider
    {
        private Dictionary<int, uint> userWithClearances = new Dictionary<int, uint>();
        private Dictionary<uint, UserClearanceData> clearances = new Dictionary<uint, UserClearanceData>();

        public bool HasClearance(int connectionId, UserClearanceLevel minimumLevel)
        {
            if (!userWithClearances.ContainsKey(connectionId))
                return false;

            if (!clearances.ContainsKey(userWithClearances[connectionId]))
                return false;

            return clearances[userWithClearances[connectionId]].ClearanceLevel >= (ushort)minimumLevel;
        }

        public string GetClearanceTitle(int connectionId)
        {
            if (!userWithClearances.ContainsKey(connectionId))
                return "";

            return !clearances.ContainsKey(userWithClearances[connectionId]) ? "" : clearances[userWithClearances[connectionId]].Title;
        }

        public UserClearanceLevel GetClearanceLevel(int connectionId)
        {
            if (!userWithClearances.ContainsKey(connectionId))
                return UserClearanceLevel.Normal;

            if (!clearances.ContainsKey(userWithClearances[connectionId]))
                return UserClearanceLevel.Normal;

            return (UserClearanceLevel)clearances[userWithClearances[connectionId]].ClearanceLevel;
        }
    }
    //CLIENT SIDE
    public class ClientCredentialProvider : ICredentialProvider, IInitializable, ILateDisposable
    {
        private readonly INetworkMessageReceiver<PlayerListMessage> listMessageReceiver;

        private Dictionary<string, uint> players = new Dictionary<string, uint>();

        public ClientCredentialProvider(INetworkMessageReceiver<PlayerListMessage> listMessageReceiver)
        {
            this.listMessageReceiver = listMessageReceiver;
        }

        public void Initialize()
        {
            listMessageReceiver.OnEventCalled += OnServerUpdated;
        }

        public void LateDispose()
        {
            listMessageReceiver.OnEventCalled -= OnServerUpdated;
        }

        public void Update()
        {

        }

        [CanBeNull]
        public NetworkIdentity GetData(string name)
        {
            return !players.ContainsKey(name) ? null : !NetworkClient.spawned.ContainsKey(players[name]) ? null : NetworkClient.spawned[players[name]];
        }

        [CanBeNull]
        public string GetData(NetworkIdentity identity)
        {
            var key = players.Where(kvp => kvp.Value == identity.netId).Take(1).ToArray();

            return string.IsNullOrEmpty(key[0].Key) ? null : key[0].Key;
        }
        public void OnPlayerAuthenticated(string name, uint netId)
        {

        }
        public void OnPlayerDisconnected(string name)
        {

        }

        private void OnServerUpdated(PlayerListMessage message)
        {
            players.Clear();

            for (int i = 0; i < message.Names.Length; i++)
            {
                players.TryAdd(message.Names[i], message.NetIds[i]);
            }
        }
    }
    //SERVER SIDE
    public class ServerCredentialProvider : ICredentialProvider
    {

        private Dictionary<string, uint> players = new Dictionary<string, uint>(); //UID is key

        public void Update()
        {
            NetworkServer.SendToReady(new PlayerListMessage(players.Keys.ToArray(), players.Values.ToArray()));
        }

        [CanBeNull]
        public NetworkIdentity GetData(string name)
        {
            return !players.ContainsKey(name) ? null : NetworkServer.spawned[players[name]];

        }
        [CanBeNull]
        public string GetData(NetworkIdentity identity)
        {
            var key = players.Where(kvp => kvp.Value == identity.netId).Take(1).ToArray();

            return string.IsNullOrEmpty(key[0].Key) ? null : key[0].Key;

        }
        public void OnPlayerAuthenticated(string name, uint netid)
        {
            players.TryAdd(name, netid);
            NetworkServer.SendToReady(new PlayerListMessage(players.Keys.ToArray(), players.Values.ToArray()));
        }
        public void OnPlayerDisconnected(string name)
        {
            if (!players.ContainsKey(name))
                return;

            players.Remove(name);
            NetworkServer.SendToReady(new PlayerListMessage(players.Keys.ToArray(), players.Values.ToArray()));
        }
    }
}