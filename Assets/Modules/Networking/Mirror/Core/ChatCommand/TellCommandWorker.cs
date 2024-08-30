using System;
using Mirror;
using UnityEngine;
using com.playbux.networking.mirror.message;
using com.playbux.identity;

namespace com.playbux.networking.mirror.core
{
    public abstract class BaseCommandWorker : ICommandWorker
    {
        public CommandInstruction Instruction { get; }

        protected BaseCommandWorker(CommandInstruction instruction)
        {
            Instruction = instruction;
        }

        internal virtual bool ValidateParameterAmount(string[] parameters)
        {
            return parameters.Length == Instruction.Parameters.Length;
        }

        internal virtual bool ValidateParameterType(string[] parameters)
        {
            int count = 0;
            bool hasCorrectAmount = ValidateParameterAmount(parameters);

            if (!hasCorrectAmount)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (Instruction.Parameters[i] == ParameterType.Text)
                {
                    if (IsText(parameters[i]))
                        count++;

                    continue;
                }

                if (Instruction.Parameters[i] == ParameterType.Number)
                {
                    if (IsNumber(parameters[i]))
                        count++;

                    continue;
                }

                if (IsDecimal(parameters[i]))
                    count++;
            }

            return count == Instruction.Parameters.Length;
        }

        internal bool IsText(string parameter)
        {
            return !string.IsNullOrEmpty(parameter);
        }

        internal bool IsNumber(string parameter)
        {
            bool isRational = int.TryParse(parameter, out int _);
            return isRational;
        }

        internal bool IsDecimal(string parameter)
        {
            bool isIrrationaal = float.TryParse(parameter, out float _);
            return isIrrationaal;
        }

        public abstract void Perform(int connectionId, params string[] parameters);
    }

    public class TellCommandWorker : BaseCommandWorker
    {
        private readonly ICredentialProvider credentialProvider;

        public TellCommandWorker(CommandInstruction instruction, ICredentialProvider credentialProvider) : base(instruction)
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

            if (parameters.Length <= 1)
                return false;

            for (int i = 1; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            return IsText(sentence);
        }

        public override void Perform(int connectionId, params string[] parameters)
        {
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
                    $"Target for {Instruction.Name} command doesnt not exist or offline.");
                connection.Send(response);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name}: target " + parameters[0] + " doesn't exist.");
#endif
                return;
            }

            if (target == connection.identity)
            {
                response = new ChatBroadcastMessage(
                    ticks,
                    4,
                    "System",
                    $"Target for {Instruction.Name} command cannot be yourself.");
                connection.Send(response);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name}: target " + parameters[0] + " cannot be the same entity as sender.");
#endif
                return;
            }

            if (!IsText(parameters[1]))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Message cannot be empty for '{Instruction.Name}' command");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: message cannot be empty.");
#endif
                return;
            }

            string sender = credentialProvider.GetData(connection.identity);
            string sentence = "";

            for (int i = 1; i < parameters.Length; i++)
            {
                sentence += parameters[i];

                if (i != parameters.Length - 1)
                    sentence += " ";
            }

            NetworkIdentity receiver = credentialProvider.GetData(parameters[0]);
            response = new ChatBroadcastMessage(ticks, (ushort)ChatLevel.Tell, sender, sentence);
            connection.Send(response);
            receiver.connectionToClient.Send(response);
        }
    }
}