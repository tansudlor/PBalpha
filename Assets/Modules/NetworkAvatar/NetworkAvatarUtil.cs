using com.playbux.entities;
using com.playbux.identity;
using com.playbux.networking.mirror.message;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.playbux.networking.networkavatar
{
    public static class NetworkAvatarUtil
    {
        // Start is called before the first frame update
        public static uint CreateAvatar(IEntity<NetworkIdentity> networkPrefab, IIdentitySystem identity,string displayName,Equipments equipments)
        {
           
            NetworkServer.Spawn(networkPrefab.GameObject);
            uint netId = networkPrefab.GameObject.GetComponent<NetworkIdentity>().netId;
            Debug.Log("netID CreateAvatar : " + netId);
            identity[netId].ID = "NETWORKOBJECT" + Guid.NewGuid();
            identity[netId].UID = "NETWORKOBJECT" + Guid.NewGuid();
            identity[netId].NetId = netId;
            identity[netId].LoginTime = DateTime.UtcNow;
            identity[netId].AccessToken = "";
            identity[netId].DisplayName = displayName;
            identity[netId].Equipments = new mirror.message.Equipments();
            identity[netId].Identity = NetworkServer.spawned[netId];
            identity[netId].BalanceBrk = 0;
            identity[netId].BalanceLottoTickets = 0;
            identity[netId].Wallet = new api.Wallet();
            identity[netId].BalancePebble = 0;
            identity[netId].CanPlayQuiz = false;
            return netId;
        }

        public static uint CreateKickToWinCircle(GameObject gameObject)
        {
            NetworkServer.Spawn(gameObject);
            uint netId = gameObject.GetComponent<NetworkIdentity>().netId;
            return netId;
        }
    }
}
