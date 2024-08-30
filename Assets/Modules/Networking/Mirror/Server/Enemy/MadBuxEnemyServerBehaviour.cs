using Mirror;
using System.Linq;
using UnityEngine;
using System.Threading;
using com.playbux.enemy;
using com.playbux.ability;
using com.playbux.effects;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.networking.server.ability;
using com.playbux.networking.server.targetable;

namespace com.playbux.networking.mirror.server.enemy
{
    public class MadBuxEnemyServerBehaviour : IEnemyServerBehaviour
    {
        private readonly EnemyEnmityList enmityList;
        private readonly EnemyIdentity enemyIdentity;
        private readonly EnemyAbilityData abilityData;
        private readonly AbilityServerFactory abilityServerFactory;
        private readonly AbilityAssetDatabase abilityAssetDatabase;
        private readonly TemporaryEffectDatabase temporaryEffectDatabase;
        private readonly ServerTargetableController targetableController;

        private uint? currentAbility;
        private CancellationTokenSource cancellationSource;

        private bool isEnabled;
        private int hitPoints = 100;
        private HashSet<uint> availableAbilityIds = new HashSet<uint>();
        private HashSet<NetworkIdentity> observerHash = new HashSet<NetworkIdentity>();
        private Dictionary<uint, float> tempEffectStack = new Dictionary<uint, float>();

        public MadBuxEnemyServerBehaviour(
            EnemyEnmityList enmityList,
            EnemyIdentity enemyIdentity,
            EnemyAbilityData abilityData,
            AbilityServerFactory abilityServerFactory,
            AbilityAssetDatabase abilityAssetDatabase,
            TemporaryEffectDatabase temporaryEffectDatabase,
            ServerTargetableController targetableController
            )
        {
            this.enmityList = enmityList;
            this.abilityData = abilityData;
            this.enemyIdentity = enemyIdentity;
            this.targetableController = targetableController;
            this.abilityServerFactory = abilityServerFactory;
            this.abilityAssetDatabase = abilityAssetDatabase;
            this.temporaryEffectDatabase = temporaryEffectDatabase;

            cancellationSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            isEnabled = true;
            availableAbilityIds = abilityData.abilities.ToHashSet();
            targetableController.Add(enemyIdentity.Id, enemyIdentity.NetworkIdentity.transform.position);
            StateLoop().Forget();
        }
        public void Dispose()
        {
            isEnabled = false;
            targetableController.Remove(enemyIdentity.Id);
        }

        private void Interrupt()
        {
            cancellationSource?.Cancel();
        }

        private async UniTaskVoid StateLoop()
        {
            // await UniTask.WaitUntil(() => enmityList.TargetPlayer.HasValue);
            await UniTask.WaitForSeconds(5f);

            while (hitPoints > 0)
            {
                await RandomCast(cancellationSource.Token);
                await Exhaust(cancellationSource.Token);

                if (!cancellationSource.IsCancellationRequested)
                    continue;

                Interrupt();
                cancellationSource = new CancellationTokenSource();
            }

            observerHash.Clear();
            NetworkServer.aoi.GetObserverAt(enemyIdentity.NetworkIdentity.transform.position, observerHash);

            foreach (var pair in observerHash)
            {
                if (enemyIdentity.NetworkIdentity.netId == pair.netId)
                    continue;
                
                pair.connectionToClient.Send(new EntityDespawnMessage(enemyIdentity.Id));
            }

            //TODO: after that set the event complete

            await UniTask.WaitForSeconds(5f);
            Interrupt();
            NetworkServer.Destroy(enemyIdentity.NetworkIdentity.gameObject);

        }

        private async UniTask RandomCast(CancellationToken cancellationToken)
        {
            if (currentAbility.HasValue)
                return;

            if (availableAbilityIds.Count <= 0)
                availableAbilityIds = abilityData.abilities.ToHashSet();

            int currentAttackIndex = Random.Range(0, availableAbilityIds.Count);
            currentAbility = availableAbilityIds.ElementAt(currentAttackIndex);

            if (!currentAbility.HasValue)
                return;

            var abilityAsset = abilityAssetDatabase.Get(currentAbility.Value);
            if (abilityAsset == null)
                return;

            if (cancellationToken.IsCancellationRequested)
                return;

            var abilityInstance = abilityServerFactory.Create(abilityAsset.asset, enemyIdentity.NetworkIdentity.transform.position);

            if (cancellationToken.IsCancellationRequested)
            {
                NetworkServer.Destroy(abilityInstance.gameObject);
                return;
            }

            availableAbilityIds.Remove(currentAbility.Value);
            abilityInstance.Ability.Cast(enemyIdentity.NetworkIdentity, enemyIdentity.NetworkIdentity.transform.position);
            await UniTask.WaitWhile(() => abilityInstance.Ability.IsCasting, PlayerLoopTiming.Update, cancellationToken);
            await UniTask.WaitForSeconds(abilityInstance.Ability.AnimationTime, false, PlayerLoopTiming.Update, cancellationToken);
            currentAbility = null;

            if (!cancellationToken.IsCancellationRequested)
                return;

            currentAbility = null;
            abilityInstance.Ability.Cancel(enemyIdentity.NetworkIdentity);
            Object.Destroy(abilityInstance.gameObject);
        }

        private async UniTask Exhaust(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await UniTask.WaitForSeconds(5f, false, PlayerLoopTiming.Update, cancellationToken);
            //TODO: need to do a debuff system
            // observerHash = NetworkServer.spawned.Values.ToHashSet();
            // NetworkServer.aoi.GetObserverAt(enemyIdentity.NetworkIdentity.transform.position, observerHash);
            // var tempEffect = temporaryEffectDatabase.Get(abilityData.statuses[0]);
            //
            // if (tempEffect == null)
            //     return;
            //
            // tempEffectStack.Add(abilityData.statuses[0], tempEffect.duration);
            //
            // foreach (var pair in observerHash)
            // {
            //     if (enemyIdentity.NetworkIdentity.netId == pair.netId)
            //         continue;
            //     
            //     pair.connectionToClient.Send(new EntityEffectUpdateMessage(enemyIdentity.Id, tempEffectStack.Keys.ToArray(), tempEffectStack.Values.ToArray()));
            // }
            //
            // await UniTask.WaitForSeconds(tempEffect.duration, false, PlayerLoopTiming.Update, cancellationToken);
            //
            // observerHash = NetworkServer.spawned.Values.ToHashSet();
            // NetworkServer.aoi.GetObserverAt(enemyIdentity.NetworkIdentity.transform.position, observerHash);
            // tempEffectStack.Remove(abilityData.statuses[0]);
            //
            // foreach (var pair in observerHash)
            // {
            //     if (enemyIdentity.NetworkIdentity.netId == pair.netId)
            //         continue;
            //     
            //     pair.connectionToClient.Send(new EntityEffectUpdateMessage(enemyIdentity.Id, tempEffectStack.Keys.ToArray(), tempEffectStack.Values.ToArray()));
            // }
        }
    }
}