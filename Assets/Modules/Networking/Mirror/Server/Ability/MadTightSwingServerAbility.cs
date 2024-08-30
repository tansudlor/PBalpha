using Mirror;
using UnityEngine;
using System.Linq;
using com.playbux.ability;
using com.playbux.identity;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.collision;

namespace com.playbux.networking.server.ability
{
    public class MadTightSwingServerAbility : ServerTargetAbilityBase
    {
        private readonly GameObject gameObject;
        private readonly CircleAreaTrigger trigger;
        private readonly IIdentitySystem identitySystem;
        
        private HashSet<NetworkIdentity> observerHash;

        public MadTightSwingServerAbility(
            uint abilityId,
            AbilityData data,
            GameObject gameObject,
            CircleAreaTrigger trigger,
            IIdentitySystem identitySystem) : base(abilityId, data)
        {
            this.trigger = trigger;
            this.gameObject = gameObject;
            this.identitySystem = identitySystem;
            this.trigger.gameObject.SetActive(false);
            observerHash = new HashSet<NetworkIdentity>();
        }
        protected override void SendStartCast(NetworkIdentity casterIdentity, NetworkIdentity targetIdentity, NetworkConnectionToClient connection)
        {
            var message = new StartCastMessage(Id, casterIdentity.netId, casterIdentity.netId, abilityId);
            connection?.Send(message);
            observerHash = NetworkServer.spawned.Values.ToHashSet();
            NetworkServer.aoi.GetObserverAt(targetIdentity.transform.position, observerHash);

            foreach (var pair in observerHash)
            {
                if (casterIdentity.netId == pair.netId)
                    continue;
                
                pair.connectionToClient?.Send(message);
            }
        }
        protected override void SendUpdateCast(float currentTime, NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            var message = new UpdateCastMessage(currentTime, Id, casterIdentity.netId, casterIdentity.netId, abilityId);
            connection?.Send(message);
            
            observerHash = NetworkServer.spawned.Values.ToHashSet();
            NetworkServer.aoi.GetObserverAt(casterIdentity.transform.position, observerHash);

            foreach (var pair in observerHash)
            {
                if (casterIdentity.netId == pair.netId)
                    continue;
                
                pair.connectionToClient?.Send(message);
            }
        }
        protected override void SendCancelCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            var message = new CancelCastMessage(Id, abilityId);
            connection?.Send(message);
            
            observerHash = NetworkServer.spawned.Values.ToHashSet();
            NetworkServer.aoi.GetObserverAt(casterIdentity.transform.position, observerHash);

            foreach (var pair in observerHash)
            {
                if (casterIdentity.netId == pair.netId)
                    continue;
                
                pair.connectionToClient?.Send(message);
            }
        }
        protected override void SendEndCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            var message = new EndCastMessage(Id, casterIdentity.netId, abilityId);
            connection?.Send(message);
            observerHash = NetworkServer.spawned.Values.ToHashSet();
            NetworkServer.aoi.GetObserverAt(casterIdentity.transform.position, observerHash);

            foreach (var pair in observerHash)
            {
                if (pair == null)
                    continue;
                
                if (pair.netId == casterIdentity.netId)
                    continue;
                
                pair.connectionToClient?.Send(message);

                if (!HitCheck(pair))
                    continue;

                OnAbilityHit(pair);
            }

            Object.Destroy(gameObject);
        }
        protected override void OnAbilityHit(NetworkIdentity otherIdentity)
        {
            if (!identitySystem.ContainsKey(otherIdentity.netId))
                return;
            
            //TODO need to do some damage or debuff on player
        }

        protected override bool HitCheck(NetworkIdentity other)
        {
            return trigger.IsTrigger(other.transform.position);
        }
    }
}