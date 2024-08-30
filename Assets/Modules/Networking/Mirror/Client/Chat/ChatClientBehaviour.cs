using System;
using Mirror;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.identity;
using com.playbux.ui.bubble;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatBubbleRuntimeWrapper
    {
        public float timer;
        public IBubble bubble;

        public ChatBubbleRuntimeWrapper(float timer, IBubble bubble)
        {
            this.timer = timer;
            this.bubble = bubble;
        }
    }

    public class ChatClientBehaviour : IClientBehaviour, IInitializable, IDisposable, IFixedTickable
    {
        private readonly IChatRecorder chatRecorder;
        private readonly Vector3 offset = Vector3.up * 10;
        private readonly ChatUIController chatUIController;
        private readonly BubbleController<Bubble.Pool> bubbleController;
        private readonly ICredentialProvider credentialProvider;
        private readonly INetworkMessageReceiver<ChatBroadcastMessage> chatMessageReceiver;

        private Dictionary<string, ChatBubbleRuntimeWrapper> bubbles = new Dictionary<string, ChatBubbleRuntimeWrapper>();
        private Dictionary<string, ChatBubbleRuntimeWrapper> disconnectList = new Dictionary<string, ChatBubbleRuntimeWrapper>();

        public ChatClientBehaviour(
            IChatRecorder chatRecorder,
            ChatUIController chatUIController,
            BubbleController<Bubble.Pool> bubbleController,
            ICredentialProvider credentialProvider,
            INetworkMessageReceiver<ChatBroadcastMessage> chatMessageReceiver)
        {
            this.chatRecorder = chatRecorder;
            this.chatUIController = chatUIController;
            this.bubbleController = bubbleController;
            this.credentialProvider = credentialProvider;
            this.chatMessageReceiver = chatMessageReceiver;
        }

        public void Initialize()
        {
            chatUIController.OnMessageSubmitted += Send;
            chatMessageReceiver.OnEventCalled += Receive;
        }
        public void Dispose()
        {
            chatUIController.OnMessageSubmitted -= Send;
            chatMessageReceiver.OnEventCalled -= Receive;
        }

        private void Send(ChatCommandMessage broadcastMessage)
        {
            NetworkClient.Send(broadcastMessage);
        }

        private void Receive(ChatBroadcastMessage broadcastMessage)
        {
            chatRecorder.Record(broadcastMessage.sender, broadcastMessage.message, (ChatLevel)broadcastMessage.chatLevel);

            if ((ChatLevel)broadcastMessage.chatLevel == ChatLevel.Warning || (ChatLevel)broadcastMessage.chatLevel == ChatLevel.Announcement)
                return;

            AddBubble(broadcastMessage);
        }

        private void AddBubble(ChatBroadcastMessage broadcastMessage)
        {
            if (bubbles.TryGetValue(broadcastMessage.sender, out var existingBubble))
            {
                existingBubble.timer = 0;
                existingBubble.bubble.UpdateText(broadcastMessage.message);
                return;
            }

            NetworkIdentity senderIdentity = credentialProvider.GetData(broadcastMessage.sender);

            if (senderIdentity == null)
                return;

            IBubble bubble = bubbleController.GetBubble(broadcastMessage.message, senderIdentity.transform.position + offset, BubbleChannel.Chat);
            var wrapper = new ChatBubbleRuntimeWrapper(0, bubble);
            bubbles.TryAdd(broadcastMessage.sender, wrapper);
        }

        public void FixedTick()
        {
            foreach (var pair in bubbles)
            {
                pair.Value.timer += Time.deltaTime;
                NetworkIdentity senderIdentity = credentialProvider.GetData(pair.Key);

                if (senderIdentity == null)
                {
                    disconnectList.Add(pair.Key, pair.Value);
                    continue;
                }

                pair.Value.bubble.UpdatePosition(senderIdentity.transform.position + offset);
            }

            foreach (var pair in disconnectList)
            {
                bubbles.Remove(pair.Key);
                bubbleController.ReturnBubble(pair.Value.bubble);
            }

            disconnectList.Clear();

            var timeOutList = bubbles.Where(bubble => bubble.Value.timer >= 4).ToArray();
            foreach (var keyPair in timeOutList)
            {
                if (disconnectList.ContainsKey(keyPair.Key))
                    continue;

                bubbles.Remove(keyPair.Key);
                bubbleController.ReturnBubble(keyPair.Value.bubble);
            }
        }
    }
}