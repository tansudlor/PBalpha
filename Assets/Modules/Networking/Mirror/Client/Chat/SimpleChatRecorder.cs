using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client.chat
{
    public class SimpleChatRecorder : IChatRecorder
    {
        public event Action<ChatBroadcastMessage> OnRecord;

        private readonly List<ChatBroadcastMessage> messages;
        public SimpleChatRecorder()
        {
            this.messages = new List<ChatBroadcastMessage>(NetworkClient.snapshotSettings.bufferLimit);
        }

        public ChatBroadcastMessage[] GetFilteredMessages(ChatLevel[] filters)
        {
            var filteredMessages = new List<ChatBroadcastMessage>();

            for (int i = 0; i < messages.Count; i++)
            {
                if (filters.All(level => (ushort)level != messages[i].chatLevel))
                    continue;

                var dateTime = new DateTime(messages[i].timestamp);
                filteredMessages.Add(new ChatBroadcastMessage(
                    dateTime.TimeOfDay.Ticks,
                    messages[i].chatLevel,
                    messages[i].sender,
                    messages[i].message));
            }

            return filteredMessages.ToArray();
        }

        public void Record(string sender, string message, ChatLevel level = ChatLevel.Say)
        {
            var msg = new ChatBroadcastMessage(DateTime.Now.Ticks, (ushort)level, sender, message);
            messages.Add(msg);
            OnRecord?.Invoke(msg);
        }
    }
}