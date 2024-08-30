using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace com.playbux.networking.mirror.core
{
    public class ChatCommandProcessor
    {
        private const char commandInitiator = '/';
        private const char parameterSeparator = ' ';
        private readonly Dictionary<CommandNameWrapper, ICommandWorker> workers;

        public ChatCommandProcessor(List<ICommandWorker> workers)
        {
            this.workers = new Dictionary<CommandNameWrapper, ICommandWorker>();

            for (int i = 0; i < workers.Count; i++)
            {
                CommandInstruction instruction = workers[i].Instruction;
                var nameWrapper = new CommandNameWrapper(instruction.Name, instruction.AltName);
                this.workers.Add(nameWrapper, workers[i]);
            }
        }
        public bool Process(int connectionId, string msgCommand)
        {
            if (msgCommand[0] != commandInitiator)
                return false;

            string name = "";

            for (int i = 1; i < msgCommand.Length; i++)
            {
                if (msgCommand[i] == parameterSeparator)
                    break;

                name += msgCommand[i];
            }

            msgCommand = msgCommand.Remove(0, 1); //NOTE: To remove backslash
            var words = msgCommand.Split(parameterSeparator).ToList();

            ICommandWorker worker = workers.FirstOrDefault(worker =>
                worker.Key.Equals(new CommandNameWrapper(name, words[0]))).Value;

            if (worker == null)
                return true; //NOTE: It is a command but the command doesn't exist or unauthorized

            words.RemoveAt(0);

            worker.Perform(connectionId, words.ToArray());
            return true;
        }
    }
}