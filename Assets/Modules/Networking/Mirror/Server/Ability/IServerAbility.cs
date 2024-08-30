using Mirror;
using Zenject;
using UnityEngine;

namespace com.playbux.networking.server.ability
{
    public interface IServerAbility
    {
        uint Id { get; }
        uint AbilityId { get; }
        bool IsCasting { get; }
        float Duration { get; }
        float AnimationTime { get; }
        void Cast(NetworkIdentity casterIdentity, Vector2 castPosition);
        void Cancel(NetworkIdentity casterIdentity);
    }

    public class AbilityServerFactory : PlaceholderFactory<GameObject, Vector2, ServerAbilityFacade>
    {

    }

    public interface IServerTargetAbility
    {
        uint Id { get; }
        uint AbilityId { get; }
        bool IsCasting { get; }
        float Duration { get; }
        void Cast(NetworkIdentity casterIdentity, NetworkIdentity targetIdentity);
        void Cancel(NetworkIdentity casterIdentity);
    }
}