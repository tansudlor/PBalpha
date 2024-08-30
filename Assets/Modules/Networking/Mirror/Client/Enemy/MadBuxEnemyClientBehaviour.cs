using Mirror;
using UnityEngine;
using com.playbux.bux;
using com.playbux.enemy;
using com.playbux.ability;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.client.ability;
using com.playbux.networking.mirror.message;
using Cysharp.Threading.Tasks;

namespace com.playbux.networking.mirror.client.enemy
{
    public class MadBuxEnemyClientBehaviour : IEnemyClientBehaviour
    {
        private readonly IAnimator animator;
        private readonly EnemyAbilityData data;
        private readonly AbilityAssetDatabase assetDatabase;
        private readonly IClientAbility.Factory abilityFactory;
        private readonly AbilityAnimationName<string> abilityAnimationName;
        private readonly INetworkMessageReceiver<EndCastMessage> endCastMessageReceiver;
        private readonly INetworkMessageReceiver<StartCastMessage> startCastMessageReceiver;
        private readonly INetworkMessageReceiver<UpdateCastMessage> updateCastMessageReceiver;
        private readonly INetworkMessageReceiver<CancelCastMessage> cancelCastMessageReceiver;
        private readonly INetworkMessageReceiver<EntityDespawnMessage> despawnMessageReceiver;
        private readonly INetworkMessageReceiver<EntityEffectUpdateMessage> effectUpdateMessageReceiver;

        private string currentAnimation;
        private EnemyIdentity enemyIdentity;
        private Dictionary<uint, IClientAbility> currentCasts = new Dictionary<uint, IClientAbility>();

        public MadBuxEnemyClientBehaviour(
            IAnimator animator,
            EnemyAbilityData data,
            AbilityAssetDatabase assetDatabase,
            IClientAbility.Factory abilityFactory,
            AbilityAnimationName<string> abilityAnimationName,
            INetworkMessageReceiver<EndCastMessage> endCastMessageReceiver,
            INetworkMessageReceiver<StartCastMessage> startCastMessageReceiver,
            INetworkMessageReceiver<UpdateCastMessage> updateCastMessageReceiver,
            INetworkMessageReceiver<CancelCastMessage> cancelCastMessageReceiver,
            INetworkMessageReceiver<EntityDespawnMessage> despawnMessageReceiver,
            INetworkMessageReceiver<EntityEffectUpdateMessage> effectUpdateMessageReceiver)
        {
            this.data = data;
            this.animator = animator;
            this.assetDatabase = assetDatabase;
            this.abilityFactory = abilityFactory;
            this.abilityAnimationName = abilityAnimationName;
            this.endCastMessageReceiver = endCastMessageReceiver;
            this.despawnMessageReceiver = despawnMessageReceiver;
            this.startCastMessageReceiver = startCastMessageReceiver;
            this.updateCastMessageReceiver = updateCastMessageReceiver;
            this.cancelCastMessageReceiver = cancelCastMessageReceiver;
            this.effectUpdateMessageReceiver = effectUpdateMessageReceiver;
        }

        public void Initialize(EnemyIdentity enemyIdentity)
        {
            this.enemyIdentity = enemyIdentity;
            endCastMessageReceiver.OnEventCalled += OnEndCastMessage;
            despawnMessageReceiver.OnEventCalled += OnDespawnMessage;
            startCastMessageReceiver.OnEventCalled += OnStartCastMessage;
            updateCastMessageReceiver.OnEventCalled += OnUpdateCastMessage;
            cancelCastMessageReceiver.OnEventCalled += OnCancelCastMessage;
            effectUpdateMessageReceiver.OnEventCalled += OnEffectUpdateMessage;
        }
        public void Dispose()
        {
            endCastMessageReceiver.OnEventCalled -= OnEndCastMessage;
            despawnMessageReceiver.OnEventCalled -= OnDespawnMessage;
            startCastMessageReceiver.OnEventCalled -= OnStartCastMessage;
            updateCastMessageReceiver.OnEventCalled -= OnUpdateCastMessage;
            cancelCastMessageReceiver.OnEventCalled -= OnCancelCastMessage;
            effectUpdateMessageReceiver.OnEventCalled -= OnEffectUpdateMessage;
            animator.Stop(currentAnimation);
        }

        public void ChangeDirection(Vector2 turnDirection)
        {

        }

        private void OnStartCastMessage(StartCastMessage message)
        {
            animator.Stop("0_Idle");

            if (currentCasts.ContainsKey(message.castId))
            {
                currentCasts[message.castId].CancelCast();
                animator.Stop(animator.GetCurrentAnimationName());
                currentCasts.Remove(message.castId);
            }

            if (message.casterId != enemyIdentity.NetworkIdentity.netId)
                return;

            int index = -1;
            for (int i = 0; i < data.abilities.Length; i++)
            {
                if (message.abilityId != data.abilities[i])
                    continue;

                index = i;
            }

            if (index < 0)
                return;

            var assetData = assetDatabase.Get(data.abilities[index]);

            if (assetData == null)
                return;

            var abilityInstance = abilityFactory.Create(assetData.asset, enemyIdentity.NetworkIdentity.transform.position);
            abilityInstance.StartCast();
            animator.Play(abilityAnimationName.data[message.abilityId][AbilityAnimationState.PreCast]);
            currentCasts.Add(message.castId, abilityInstance);
        }

        private void OnUpdateCastMessage(UpdateCastMessage message)
        {
            animator.Stop("0_Idle");

            if (!currentCasts.ContainsKey(message.castId))
            {
                var assetData = assetDatabase.Get(message.abilityId);

                if (assetData == null)
                    return;

                var abilityInstance = abilityFactory.Create(assetData.asset, enemyIdentity.NetworkIdentity.transform.position);
                abilityInstance.StartCast();
                animator.Play(abilityAnimationName.data[message.abilityId][AbilityAnimationState.PreCast]);
                currentCasts.Add(message.castId, abilityInstance);
                return;
            }

            float castTime = message.currentCastTime;
            float pong = (float)(NetworkTime.rtt * 0.5f);
            float diff = (currentCasts[message.castId].CastTime + pong) - currentCasts[message.castId].CastTime;

            if (diff < pong)
                castTime += diff;
            else
                castTime += pong;

            currentCasts[message.castId].UpdateCast(castTime);
        }

        private void OnCancelCastMessage(CancelCastMessage message)
        {
            if (!currentCasts.ContainsKey(message.castId))
                return;

            currentCasts[message.castId].CancelCast();
            animator.Stop(abilityAnimationName.data[message.abilityId][AbilityAnimationState.PreCast]);
            animator.Play("0_Idle", true);
            currentCasts.Remove(message.castId);
        }

        private void OnEndCastMessage(EndCastMessage message)
        {
            if (!currentCasts.ContainsKey(message.castId))
                return;

            currentCasts[message.castId].EndCast();
            animator.Stop(abilityAnimationName.data[message.abilityId][AbilityAnimationState.PreCast]);
            animator.Play(abilityAnimationName.data[message.abilityId][AbilityAnimationState.Cast], true);
            StopAfter(2f, abilityAnimationName.data[message.abilityId][AbilityAnimationState.Cast]).Forget();
            PlayAfter(2.1f, "00_Idle", true).Forget();
            currentCasts.Remove(message.castId);
        }

        private void OnDespawnMessage(EntityDespawnMessage message)
        {

        }

        private void OnEffectUpdateMessage(EntityEffectUpdateMessage message)
        {

        }

        private async UniTaskVoid StopAfter(float delay, string animationName)
        {
            await UniTask.WaitForSeconds(delay);
            animator.Stop(animationName);
        }

        private async UniTaskVoid PlayAfter(float delay, string animationName, bool loop)
        {
            await UniTask.WaitForSeconds(delay);
            animator.Play(animationName, loop);
        }
    }
}