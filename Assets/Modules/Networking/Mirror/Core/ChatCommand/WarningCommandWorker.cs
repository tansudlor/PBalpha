using System;
using Mirror;
using UnityEngine;
using com.playbux.networking.mirror.message;
using com.playbux.identity;

namespace com.playbux.networking.mirror.core
{

    public class WarningCommandWorker : BaseCommandWorker
    {
        private readonly ICredentialProvider credentialProvider;
        private readonly UserLevelClearanceProvider clearanceProvider;

        public WarningCommandWorker(CommandInstruction instruction, UserLevelClearanceProvider clearanceProvider, ICredentialProvider credentialProvider) : base(instruction)
        {
            this.clearanceProvider = clearanceProvider;
            this.credentialProvider = credentialProvider;
        }

        public override void Perform(int connectionId, params string[] parameters)
        {
            if (clearanceProvider.HasClearance(connectionId, UserClearanceLevel.Moderator))
                return;

            if (!NetworkServer.connections.ContainsKey(connectionId))
                return;

            ChatBroadcastMessage response;
            long ticks = DateTime.Now.Ticks;
            NetworkConnectionToClient connection = NetworkServer.connections[connectionId];

            if (!ValidateParameterAmount(parameters))
            {
                response = new ChatBroadcastMessage(
                    ticks,
                    4,
                    "System",
                    $"Invalid parameters for '{Instruction.Name}' command.");
                connection.Send(response);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name}: Invalid parameters.");
#endif

                return;
            }

            string name = credentialProvider.GetData(connection.identity);

            if (string.IsNullOrEmpty(name))
            {
#if DEVELOPMENT
                Debug.Log($"{Instruction.Name}: Sender disconnected.");
#endif
                return;
            }

            NetworkIdentity target = credentialProvider.GetData(parameters[0]);

            if (target == null)
            {
                response = new ChatBroadcastMessage(
                    ticks,
                    4,
                    "System",
                    $"Target to use a '{Instruction.Name}' command doesnt not exist or offline.");
                connection.Send(response);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name}: target " + parameters[0] + " doesn't exist.");
#endif
                return;
            }

            if (!IsText(parameters[1]))
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

            string sentence = "";

            for (int i = 1; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            NetworkIdentity receiver = credentialProvider.GetData(parameters[0]);
            response = new ChatBroadcastMessage(ticks, (ushort)ChatLevel.Warning, "System", sentence);
            connection.Send(response);
            receiver.connectionToClient.Send(response);
        }
    }
}