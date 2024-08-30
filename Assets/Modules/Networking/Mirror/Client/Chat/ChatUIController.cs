using System;
using Zenject;
using Mirror;
using UnityEngine;
using System.Linq;
using com.playbux.events;
using com.playbux.identity;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using com.playbux.ui.sortable;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatUIController : IInitializable, ILateDisposable
    {
        public event Action<ChatCommandMessage> OnMessageSubmitted;

        private readonly int chatLimit = NetworkClient.snapshotSettings.bufferLimit * 4 - 1;
        private readonly SignalBus signalBus;
        private readonly SortableUI sortableUI;
        private readonly ChatMessageUI.Pool pool;
        private readonly ChatUIFader chatUIFader;
        private readonly Transform messageContainer;
        private readonly IChatRecorder chatRecorder;
        private readonly ChatSpamDetector spamDetector;
        private readonly ChatInputController inputController;
        private readonly ICredentialProvider credentialProvider;
        private readonly ChatTabButtonController buttonController;
        private readonly ChatConstrainValidator constrainValidator;
        private readonly IIdentitySystem identitySystem;
        private CanvasGroup canvasGroup;

        private readonly ChatTabSettings[] settings;
        private readonly List<ChatMessageUI> activeMessages;

        private bool isOpened;
        private ChatTabSettings currentTab;
        private ChatLevel currentSendLevel = ChatLevel.Say;

        public ChatUIController(
            SignalBus signalBus,
            ChatMessageUI.Pool pool,
            ChatUIFader chatUIFader,
            SortableUI sortableUI,
            IChatRecorder chatRecorder,
            Transform messageContainer,
            ChatSpamDetector spamDetector,
            ChatInputController inputController,
            ICredentialProvider credentialProvider,
            ChatTabButtonController buttonController,
            ChatConstrainValidator constrainValidator,
            ChatTabSettings[] settings,
            IIdentitySystem identitySystem,
            CanvasGroup canvasGroup)
        {
            this.pool = pool;
            this.settings = settings;
            this.signalBus = signalBus;
            this.chatUIFader = chatUIFader;
            this.chatRecorder = chatRecorder;
            this.spamDetector = spamDetector;
            this.sortableUI = sortableUI;
            this.inputController = inputController;
            this.messageContainer = messageContainer;
            this.buttonController = buttonController;
            this.credentialProvider = credentialProvider;
            this.constrainValidator = constrainValidator;
            this.identitySystem = identitySystem;
            this.canvasGroup = canvasGroup;
            activeMessages = new List<ChatMessageUI>(NetworkClient.snapshotSettings.bufferLimit);
        }

        public void Initialize()
        {
            isOpened = true;
            currentTab = settings[0];
            buttonController.Tabs[0].SetActive(true);

            inputController.OnWokeUp += Open;
            inputController.OnCanceled += Close;
            chatRecorder.OnRecord += OnMessageReceived;
            inputController.OnTypeSubmitted += OnTypeSubmitted;

            Close();
        }

        public void LateDispose()
        {
            pool.Clear();

            inputController.OnWokeUp -= Open;
            inputController.OnCanceled -= Close;
            chatRecorder.OnRecord -= OnMessageReceived;
            inputController.OnTypeSubmitted -= OnTypeSubmitted;
        }

        public void Open()
        {
            if (isOpened)
                return;

            isOpened = true;

            chatUIFader.FadeIn();
            //FIXME: this make other ui cant interact : inventory
            //Bug on Production
            canvasGroup.blocksRaycasts = true;
            //sortableUI.BringToTop();
        }

        public void Close()
        {
            if (!isOpened)
                return;

            isOpened = false;

            chatUIFader.FadeOut();
            //FIXME: this make other ui cant interact : inventory 
            //Bug on Production
            canvasGroup.blocksRaycasts = false;
            //inputController.Cancel();
        }

        public void SetFocus(bool becomeFocusing)
        {
            if (becomeFocusing)
            {
                Open();
                inputController.Enable();
                return;
            }

            Close();
            inputController.Disable();
        }

        private void RefreshCurrentTab()
        {
            for (int i = 0; i < activeMessages.Count; i++)
                pool.Despawn(activeMessages[i]);

            activeMessages.Clear();

            var filteredMessages = chatRecorder.GetFilteredMessages(currentTab.filter);
            int startingIndex = 0;

            if (filteredMessages.Length >= chatLimit)
                startingIndex = filteredMessages.Length - chatLimit;

            for (int i = startingIndex; i < filteredMessages.Length; i++)
            {
                activeMessages.Add(pool.Spawn(
                    ConstructString(filteredMessages[i]),
                    (ChatLevel)filteredMessages[i].chatLevel,
                    messageContainer));
            }
        }

        private string ConstructString(ChatBroadcastMessage broadcastMessage)
        {
            var timestamp = new DateTime(broadcastMessage.timestamp);
            uint netId = identitySystem.NameReverse[broadcastMessage.sender];
            return $"[{timestamp:t}][{identitySystem[netId].DisplayName}]:{broadcastMessage.message}";
        }

        private void OnTypeSubmitted(string message)
        {
            sortableUI.BringToTop();
            bool canBeSubmitted = spamDetector.ValidateSpam();

            if (canBeSubmitted)
            {
                bool hasRepeatedSpace = false;

                if (message.Length >= 2)
                {
                    for (int i = 0; i < message.Length - 1; i++)
                    {
                        if (message[i] != ' ')
                            continue;

                        if (message[i] == message[i + 1])
                        {
                            hasRepeatedSpace = true;
                        }
                    }
                }

                if (hasRepeatedSpace)
                {
                    activeMessages.Add(pool.Spawn("Invalid character detected, message cancelled.", ChatLevel.Warning, messageContainer));
                    return;
                }

                string finalMessage = "";

                if (message[0] != '/')
                {
                    finalMessage += '/';

                    switch (currentSendLevel)
                    {
                        case ChatLevel.Say:
                            finalMessage += 's';
                            break;
                        case ChatLevel.Shout:
                            finalMessage += "sh";
                            break;
                        case ChatLevel.Tell:
                            finalMessage += 't';
                            break;
                        default:
                            finalMessage += 's';
                            break;
                    }

                    finalMessage += ' ';
                    finalMessage += message;
                }
                else if (message[0] == '/')
                {
                    bool notEmpty = !string.IsNullOrEmpty(message) || message.Length > 0;
                    bool hasMoreThanOneChar = message.Length > 1;
                    var level = ChatLevel.Say;

                    if (notEmpty && message[1] == 's')
                        level = ChatLevel.Say;

                    if (hasMoreThanOneChar && message[1] == 's' && message[2] == 'h')
                        level = ChatLevel.Shout;

                    if (notEmpty && message[1] == 't')
                        level = ChatLevel.Tell;

                    signalBus.Fire(new UserChatLevelChangeSignal(level));
                }

                string trimmed = constrainValidator.Trim(string.IsNullOrEmpty(finalMessage) ? message : finalMessage);

                string messageForValidate = string.IsNullOrEmpty(finalMessage) ? message : finalMessage;
                message.Remove(0);
                message.Remove(0);
                if (constrainValidator.HasProhibitedWord(messageForValidate))
                {
                    activeMessages.Add(pool.Spawn("Your message contains prohibited word.",
                        ChatLevel.Warning,
                        messageContainer));

                    return;
                }

                var chatMessage = new ChatCommandMessage(
                    DateTime.Now.Ticks,
                    credentialProvider.GetData(NetworkClient.localPlayer),
                    trimmed
                    );

                OnMessageSubmitted?.Invoke(chatMessage);
                return;
            }

            string penaltyTime = spamDetector.PenaltyTime.ToString("F0");
            activeMessages.Add(pool.Spawn($"Cannot send message right now, try again in {penaltyTime} seconds.",
                ChatLevel.Warning,
                messageContainer));
        }

        private void OnMessageReceived(ChatBroadcastMessage broadcastMessage)
        {
            var receivedLevel = (ChatLevel)broadcastMessage.chatLevel;

            for (int i = 0; i < buttonController.Tabs.Length; i++)
            {
                if (buttonController.Tabs[i].Name == currentTab.name)
                    continue;

                if (buttonController.Tabs[i].Filter.All(level => level != receivedLevel))
                    continue;

                if (broadcastMessage.chatLevel != (ushort)ChatLevel.Warning && broadcastMessage.chatLevel != (ushort)ChatLevel.Announcement)
                    buttonController.Tabs[i].SetNotification(true);
            }

            if (currentTab.filter.All(level => level != receivedLevel))
                return;

            if (activeMessages.Count >= chatLimit)
            {
                pool.Despawn(activeMessages[0]);
                activeMessages.RemoveAt(0);
            }

            activeMessages.Add(pool.Spawn(ConstructString(broadcastMessage), receivedLevel, messageContainer));
        }

        public void OnTabChanged(UserTabChangeSignal signal)
        {
            for (int i = 0; i < buttonController.Tabs.Length; i++)
                buttonController.Tabs[i].SetActive(signal.Index == i);

            if (currentTab == settings[signal.Index])
                return;

            buttonController.Tabs[signal.Index].SetNotification(false);
            currentTab = settings[signal.Index];
            RefreshCurrentTab();
        }

        public void OnSendLevelChanged(UserChatLevelChangeSignal signal)
        {
            currentSendLevel = signal.Level;
        }

        public void ConversationDialogOpened(ConversationDialogSignal signal)
        {
            Close();
        }
    }
}