using TMPro;
using Zenject;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.events;
using com.playbux.ui.sortable;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatUIInstaller : MonoInstaller<ChatUIInstaller>
    {
        [Inject]
        private ChatConfig chatConfig;

        [SerializeField]
        private Button button;

        [SerializeField]
        private float fadeDuration;

        [SerializeField]
        private Ease fadeEase;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private TMP_InputField inputField;

        [SerializeField]
        private Transform messageContainer;

        [SerializeField]
        private ChatMessageUI messageUIPrefab;

        [SerializeField]
        private ChatTabButtonInstaller chatTabButtonInstaller;

        [SerializeField]
        private ChatTabSettings[] settings;

        public override void InstallBindings()
        {
            Container.Bind<Dictionary<ChatLevel, Color>>().FromMethod(GetMessageColorDict).AsSingle();
            Container.Bind<ChatUIFader>().AsSingle();
            Container.Bind<float>().FromInstance(fadeDuration).AsSingle();
            Container.Bind<Ease>().FromInstance(fadeEase).AsSingle();
            Container.Bind<CanvasGroup>().FromInstance(canvasGroup).AsSingle();
            Container.BindInterfacesAndSelfTo<ChatUIController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ChatInputController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ChatSpamDetector>().AsSingle();
            Container.Bind<ChatConstrainValidator>().AsSingle();
            Container.Bind<ChatTabButtonController>()
                .FromSubContainerResolve()
                .ByNewContextPrefab(chatTabButtonInstaller)
                .UnderTransform(canvasGroup.transform)
                .AsSingle();

            Container.Bind<Button>().FromInstance(button).AsSingle();
            Container.Bind<TMP_InputField>().FromInstance(inputField).AsSingle();
            Container.Bind<ChatTabSettings[]>().FromInstance(settings).AsSingle();
            Container.Bind<Transform>().FromInstance(messageContainer).AsSingle();
            // Container.Bind<ChatPenaltySettings>().FromInstance(penaltySettings).AsSingle();

            Container.BindMemoryPool<ChatMessageUI, ChatMessageUI.Pool>()
                .WithMaxSize(128).FromComponentInNewPrefab(messageUIPrefab).UnderTransformGroup("MessageUI");

            Container.DeclareSignal<UserTabChangeSignal>();
            Container.DeclareSignal<UserChatLevelChangeSignal>();

            Container.BindSignal<UserTabChangeSignal>()
                .ToMethod<ChatUIController>(c => c.OnTabChanged).FromResolve();
            Container.BindSignal<UserChatLevelChangeSignal>()
                .ToMethod<ChatUIController>(c => c.OnSendLevelChanged).FromResolve();

            Container.BindSignal<ConversationDialogSignal>()
                .ToMethod<ChatUIController>(c => c.ConversationDialogOpened).FromResolve();
        }

        private Dictionary<ChatLevel, Color> GetMessageColorDict()
        {
            var dict = new Dictionary<ChatLevel, Color>();
            for (int i = 0; i < chatConfig.MessageColors.Length; i++)
            {
                dict.Add(chatConfig.MessageColors[i].level, chatConfig.MessageColors[i].color);
            }

            return dict;
        }
    }
}