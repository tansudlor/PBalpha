using System;
using Mirror;
using UnityEngine;
using System.Threading;
using com.playbux.ability;
using Cysharp.Threading.Tasks;

namespace com.playbux.networking.server.ability
{
    public abstract class ServerTargetAbilityBase : IServerAbility
    {
        public uint Id { get; }
        public uint AbilityId => abilityId;
        public bool IsCasting => isCasting;
        public float Duration { get; }
        public float AnimationTime { get; }

        protected readonly uint abilityId;
        protected readonly AbilityData data;

        protected bool isCasting;
        protected CancellationTokenSource cancellationTokenSource;

        public ServerTargetAbilityBase(uint abilityId, AbilityData data)
        {
            this.data = data;
            this.abilityId = abilityId;

            Duration = data.castTime;
            AnimationTime = data.animationTime;
            Id = (uint)Guid.NewGuid().GetHashCode();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Cast(NetworkIdentity casterIdentity, Vector2 castPosition)
        {
            if (casterIdentity == null)
                return;

            isCasting = true;
            
            SendStartCast(casterIdentity, casterIdentity, casterIdentity.connectionToClient);
            Cast(casterIdentity, cancellationTokenSource.Token).Forget();

#if DEVELOPMENT
            Debug.Log($"Start casting {data.name} by {casterIdentity.netId}");
#endif
        }

        public void Cancel(NetworkIdentity casterIdentity)
        {
            if (casterIdentity == null)
                return;

            if (!isCasting)
                return;

            foreach (var pair in casterIdentity.observers)
            {
                SendCancelCast(casterIdentity, pair.Value);
            }

#if DEVELOPMENT
            Debug.Log($"Cancel casting {data.name} by {casterIdentity.netId}");
#endif

            isCasting = false;
            cancellationTokenSource.Cancel();
        }

        protected async UniTaskVoid Cast(NetworkIdentity identity, CancellationToken cancellationToken)
        {
            float count = 0;

            while (count < Duration)
            {
                await UniTask.Delay(100, DelayType.DeltaTime, PlayerLoopTiming.TimeUpdate, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                count += 0.1f;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                await UniTask.Yield();
                return;
            }

            isCasting = false;
            SendEndCast(identity, identity.connectionToClient);
        }
        protected abstract void SendStartCast(NetworkIdentity casterIdentity, NetworkIdentity targetIdentity, NetworkConnectionToClient connection);
        protected abstract void SendUpdateCast(float currentTime, NetworkIdentity casterIdentity, NetworkConnectionToClient connection);
        protected abstract void SendCancelCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection);
        protected abstract void SendEndCast(NetworkIdentity casterIdentity, NetworkConnectionToClient connection);
        protected abstract void OnAbilityHit(NetworkIdentity otherIdentity);
        protected abstract bool HitCheck(NetworkIdentity other);
    }
}