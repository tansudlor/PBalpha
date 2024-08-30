using System;
using Mirror;
using UnityEngine;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.core
{
    public class KickCommandWorker : BaseCommandWorker
    {
        private readonly ICredentialProvider credentialProvider;

        public KickCommandWorker(CommandInstruction instruction, ICredentialProvider credentialProvider) : base(instruction)
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
            // if (clearanceProvider.HasClearance(connectionId))
            //     return;

            long ticks = DateTime.Now.Ticks;
            NetworkConnectionToClient connection = NetworkServer.connections[connectionId];

            if (parameters.Length < 1 || string.IsNullOrEmpty(parameters[0]))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Message cannot be empty for '{Instruction.Name}' command");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: message cannot be empty");
#endif
                return;
            }

            //TODO: needed to added a clearance check to this command

            string name = "";

            for (int i = 0; i < parameters.Length; i++)
            {
                name += parameters[i];

                if (i != parameters.Length - 1)
                    name += " ";
            }

            var target = credentialProvider.GetData(name);

            if (target == null)
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Target {name} does not exist or currently offline for '{Instruction.Name}' command");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"Target {name} does not exist or currently offline for '{Instruction.Name}' command");
#endif

                return;
            }

            target.connectionToClient.Disconnect();
            DelayAnnounce(ticks, target).Forget();
        }

        private async UniTaskVoid DelayAnnounce(long ticks, NetworkIdentity target)
        {
            string targetName = credentialProvider.GetData(target);

            var message = new ChatBroadcastMessage(
                ticks,
                (ushort)ChatLevel.Announcement,
                "System",
                $"User {targetName} has been kicked out from the server.");

            await UniTask.Delay(330);

            NetworkServer.SendToReady(message);
        }
    }
}