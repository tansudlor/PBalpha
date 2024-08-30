using System;
using Mirror;
using UnityEngine;
using com.playbux.networking.mirror.message;
using com.playbux.identity;

namespace com.playbux.networking.mirror.core
{
    public class AnnoucementCommandWorker : BaseCommandWorker
    {
        private readonly UserLevelClearanceProvider clearanceProvider;

        public AnnoucementCommandWorker(CommandInstruction instruction, UserLevelClearanceProvider clearanceProvider) : base(instruction)
        {
            this.clearanceProvider = clearanceProvider;
        }

        public override void Perform(int connectionId, params string[] parameters)
        {
            if (clearanceProvider.HasClearance(connectionId, UserClearanceLevel.Moderator))
                return;

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

            string sentence = "";

            for (int i = 0; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            var message = new ChatBroadcastMessage(
                ticks,
                (ushort)ChatLevel.Announcement,
                clearanceProvider.GetClearanceTitle(connectionId),
                sentence);

            NetworkServer.SendToReady(message);
        }
    }
}