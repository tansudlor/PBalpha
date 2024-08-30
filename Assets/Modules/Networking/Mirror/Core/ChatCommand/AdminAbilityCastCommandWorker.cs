using System;
using Mirror;
using UnityEngine;
using com.playbux.ability;
using com.playbux.identity;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.server.ability;

namespace com.playbux.networking.mirror.core
{
    public class AdminAbilityCastCommandWorker : BaseCommandWorker
    {
        private readonly ICredentialProvider credentialProvider;
        private readonly AbilityServerFactory abilityServerFactory;
        private readonly AbilityAssetDatabase abilityAssetDatabase;
        public AdminAbilityCastCommandWorker(
            CommandInstruction instruction,
            // ICredentialProvider credentialProvider,
            AbilityAssetDatabase abilityAssetDatabase,
            AbilityServerFactory abilityServerFactory) : base(instruction)
        {
            // this.credentialProvider = credentialProvider;
            this.abilityAssetDatabase = abilityAssetDatabase;
            this.abilityServerFactory = abilityServerFactory;
        }

        internal override bool ValidateParameterAmount(string[] parameters)
        {
            return parameters.Length >= Instruction.Parameters.Length;
        }

        internal override bool ValidateParameterType(string[] parameters)
        {
            return parameters.Length >= 1 && IsNumber(parameters[0]);
        }

        public override void Perform(int connectionId, params string[] parameters)
        {
            long ticks = DateTime.Now.Ticks;
            NetworkConnectionToClient connection = NetworkServer.connections[connectionId];

            if (!ValidateParameterAmount(parameters) || !ValidateParameterType(parameters))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Invalid parameter input for {Instruction.Name} command.");
                connection.Send(errorMessage);

                return;
            }

            if (parameters.Length > 0 && string.IsNullOrEmpty(parameters[0]))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Message cannot be empty for '{Instruction.Name}' command.");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: message cannot be empty.");
#endif
                return;
            }

            uint id = uint.Parse(parameters[0]);
            var assetData = abilityAssetDatabase.Get(id);

            if (assetData == null)
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Invalid parameter input for {Instruction.Name} command.");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: ability id does not exist.");
#endif
                return;
            }

            if (connection.identity is null)
                return;

            var position = connection.identity.transform.position;
            var abilityObject = abilityServerFactory.Create(assetData.asset, position);
            abilityObject.Ability.Cast(connection.identity, position);
        }
    }
}