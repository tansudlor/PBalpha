using System;
using Mirror;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using UnityEngine;
using Zenject;
using com.playbux.identity;

namespace com.playbux.networking.mirror.server.chat
{
    public class ChatSeverBehaviour : IServerBehaviour, IInitializable, ILateDisposable
    {
        private readonly ICredentialProvider credentialProvider;
        private readonly ChatCommandProcessor chatCommandProcessor;
        private readonly IServerNetworkMessageReceiver<ChatCommandMessage> serverNetworkMessageReceiver;

        public ChatSeverBehaviour(
            ICredentialProvider credentialProvider,
            ChatCommandProcessor chatCommandProcessor,
            IServerNetworkMessageReceiver<ChatCommandMessage> serverNetworkMessageReceiver)
        {
            this.credentialProvider = credentialProvider;
            this.chatCommandProcessor = chatCommandProcessor;
            this.serverNetworkMessageReceiver = serverNetworkMessageReceiver;
        }

        public void Initialize()
        {
            serverNetworkMessageReceiver.OnEventCalled += OnMessageReceived;
        }
        public void Dispose()
        {
            serverNetworkMessageReceiver.OnEventCalled -= OnMessageReceived;
        }

        public void LateDispose()
        {
            Dispose();
        }

        private void OnMessageReceived(NetworkConnectionToClient connectionToClient, ChatCommandMessage message, int channelId)
        {
#if DEVELOPMENT
            Debug.Log("Received chat message");
#endif

            bool isCommand = chatCommandProcessor.Process(connectionToClient.connectionId, message.message);

            if (isCommand)
                return;

            NetworkIdentity identity = credentialProvider.GetData(message.sender);
            var broadcastMessage = new ChatBroadcastMessage(DateTime.Now.Ticks, 0, message.sender, message.message);
            NetworkServer.SendToReadyObservers(identity, broadcastMessage);
        }
    }
}