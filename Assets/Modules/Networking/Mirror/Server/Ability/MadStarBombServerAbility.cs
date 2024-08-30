using com.playbux.ability;
using Mirror;
using UnityEngine;
namespace com.playbux.networking.server.ability
{
    public class MadStarBombServerTargetAbility : ServerTargetAbilityBase
    {
        public MadStarBombServerTargetAbility(uint abilityId, AbilityData data) : base(abilityId, data)
        {

        }
        protected override void SendStartCast(NetworkIdentity casterIdentity, NetworkIdentity targetIdentity, NetworkConnectionToClient connection)
        {
            throw new System.NotImplementedException();
        }
        protected override void SendUpdateCast(float currentTime, NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            throw new System.NotImplementedException();
        }
        protected override void SendCancelCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            throw new System.NotImplementedException();
        }
        protected override void SendEndCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection)
        {
            throw new System.NotImplementedException();
        }
        protected override void OnAbilityHit(NetworkIdentity otherIdentity)
        {
            throw new System.NotImplementedException();
        }
        protected override bool HitCheck(NetworkIdentity other)
        {
            throw new System.NotImplementedException();
        }
    }
}