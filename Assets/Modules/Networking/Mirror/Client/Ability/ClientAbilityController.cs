using System;
using Zenject;
using UnityEngine;
using com.playbux.ability;
using com.playbux.identity;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using Mirror;

namespace com.playbux.networking.client.ability
{
    public class ClientAbilityController : IInitializable, ILateDisposable
    {
        private readonly AbilityDatabase database;
        private readonly IClientAbility.Factory factory;
        private readonly IIdentitySystem identitySystem;
        private readonly AbilityAssetDatabase assetDatabase;
        private readonly INetworkMessageReceiver<EndCastMessage> endCastMessageReceiver;
        private readonly INetworkMessageReceiver<StartCastMessage> startCastMessageReceiver;
        private readonly INetworkMessageReceiver<UpdateCastMessage> updateCastMessageReceiver;
        private readonly INetworkMessageReceiver<CancelCastMessage> cancelCastMessageReceiver;
        private readonly Dictionary<uint, IClientAbility> abilities = new Dictionary<uint, IClientAbility>();

        public ClientAbilityController(
            AbilityDatabase database,
            IClientAbility.Factory factory,
            IIdentitySystem identitySystem,
            AbilityAssetDatabase assetDatabase,
            INetworkMessageReceiver<EndCastMessage> endCastMessageReceiver,
            INetworkMessageReceiver<StartCastMessage> startCastMessageReceiver,
            INetworkMessageReceiver<UpdateCastMessage> updateCastMessageReceiver,
            INetworkMessageReceiver<CancelCastMessage> cancelCastMessageReceiver)
        {
            this.factory = factory;
            this.database = database;
            this.assetDatabase = assetDatabase;
            this.identitySystem = identitySystem;
            this.endCastMessageReceiver = endCastMessageReceiver;
            this.startCastMessageReceiver = startCastMessageReceiver;
            this.updateCastMessageReceiver = updateCastMessageReceiver;
            this.cancelCastMessageReceiver = cancelCastMessageReceiver;
        }
        public void Initialize()
        {
            endCastMessageReceiver.OnEventCalled += OnEndCast;
            startCastMessageReceiver.OnEventCalled += OnStartCast;
            updateCastMessageReceiver.OnEventCalled += OnUpdateCast;
            cancelCastMessageReceiver.OnEventCalled += OnCancelCast;
        }

        public void LateDispose()
        {
            endCastMessageReceiver.OnEventCalled -= OnEndCast;
            startCastMessageReceiver.OnEventCalled -= OnStartCast;
            updateCastMessageReceiver.OnEventCalled -= OnUpdateCast;
            cancelCastMessageReceiver.OnEventCalled -= OnCancelCast;
        }

        private void OnEndCast(EndCastMessage message)
        {
            if (!abilities.ContainsKey(message.castId))
                return;

#if DEVELOPMENT
            var abilityData = database.Get(message.abilityId);

            if (abilityData is not null)
                Debug.Log($"End casting ability {abilityData.name}");
#endif

            abilities[message.castId].EndCast();
            //TODO: need to do ability animation
        }

        private void OnLocalStartCast(uint abilityId)
        {
            uint castId = (uint)Guid.NewGuid().GetHashCode();

            if (abilities.ContainsKey(castId))
                return;

#if DEVELOPMENT
            var abilityData = database.Get(abilityId);

            if (abilityData is not null)
                Debug.Log($"Start casting ability {abilityData.name}");
#endif
            //TODO: need targeting system
            // CreateAbilityInstance(abilityId, castId, message.targetId);
        }

        private void OnStartCast(StartCastMessage message)
        {
            if (abilities.ContainsKey(message.castId))
                return;

#if DEVELOPMENT
            var abilityData = database.Get(message.abilityId);

            if (abilityData is not null)
                Debug.Log($"Start casting ability {abilityData.name}");
#endif
            CreateAbilityInstance(message.abilityId, message.castId, message.targetId);
        }

        private void OnUpdateCast(UpdateCastMessage message)
        {
            if (!abilities.ContainsKey(message.castId))
                CreateAbilityInstance(message.abilityId, message.castId, message.targetId);

#if DEVELOPMENT
            var abilityData = database.Get(message.abilityId);

            if (abilityData is not null)
                Debug.Log($"Update casting ability {abilityData.name}");
#endif

            abilities[message.castId].UpdateCast(message.currentCastTime);
        }

        private void OnCancelCast(CancelCastMessage message)
        {
            if (!abilities.ContainsKey(message.castId))
                return;

#if DEVELOPMENT
            var abilityData = database.Get(message.abilityId);

            if (abilityData is not null)
                Debug.Log($"Cancel casting ability {abilityData.name}");
#endif

            abilities[message.castId].CancelCast();
        }

        private void CreateAbilityInstance(uint abilityId, uint castId, uint targetId)
        {
            var assetData = assetDatabase.Get(abilityId);
            if (assetData?.asset is null)
                return;

            NetworkIdentity identity = null;

            if (identitySystem.ContainsKey(targetId))
                identity = identitySystem[targetId].Identity;

            if (NetworkClient.spawned.TryGetValue(targetId, out var value))
                identity = value;

            if (abilities.ContainsKey(castId))
                return;
            
            if (identity == null)
                return;

            var instance = factory.Create(assetData.asset, identity.transform.position);
            abilities.Add(castId, instance);
            instance.StartCast();
            
#if DEVELOPMENT
            Debug.Log($"Creating ability [{abilityId}] with cast id of [{castId}]");
#endif
        }
    }
}