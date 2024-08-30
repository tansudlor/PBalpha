using System;
using Mirror;
using UnityEngine;
using com.playbux.networking.mirror.message;
using com.playbux.identity;

namespace com.playbux.networking.mirror.core
{
    public class ShoutCommandWorker : BaseCommandWorker
    {
        private readonly ICredentialProvider credentialProvider;

        public ShoutCommandWorker(CommandInstruction instruction, ICredentialProvider credentialProvider) : base(instruction)
        {
            this.credentialProvider = credentialProvider;
        }

        internal override bool ValidateParameterAmount(string[] parameters)
        {
            return parameters.Length >= Instruction.Parameters.Length;
        }

        internal override bool ValidateParameterType(string[] parameters)
        {
            string sentence = "";

            for (int i = 0; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            return IsText(sentence);
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


            string sender = credentialProvider.GetData(connection.identity);

            string sentence = "";

            for (int i = 0; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            var message = new ChatBroadcastMessage(ticks, (ushort)ChatLevel.Shout, sender, sentence);
            NetworkServer.SendToReady(message);
        }
    }
}