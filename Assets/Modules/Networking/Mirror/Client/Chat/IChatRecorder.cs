using System;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client.chat
{
    public interface IChatRecorder
    {
        event Action<ChatBroadcastMessage> OnRecord;
        ChatBroadcastMessage[] GetFilteredMessages(ChatLevel[] filters);
        void Record(string sender, string message, ChatLevel level = ChatLevel.Say);
    }
}