using TMPro;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{

    public class ChatLevelSelectButton : IInitializable, ILateDisposable
    {
        private readonly ChatConfig config;
        private readonly ChatLevelSelectButtonFacade facade;
        private readonly ChatLevelPalette.Factory factory;
        private readonly LinePlaceholderFactory lineFactory;

        public ChatLevelSelectButton(
            ChatConfig config,
            ChatLevelPalette.Factory factory,
            ChatLevelSelectButtonFacade facade,
            LinePlaceholderFactory lineFactory,
            ChatLevelAndCommand[] levels)
        {
            this.config = config;
            this.facade = facade;
            this.factory = factory;
            this.lineFactory = lineFactory;

            for (int i = 0; i < levels.Length; i++)
            {
                if (!levels[i].enabled)
                    continue;

                var palette = this.factory.Create();
                palette.Initialize(levels[i], this.facade.ChatModeSlot.transform, OnSelectModeClicked);

                if (i >= levels.Length - 1)
                    continue;

                lineFactory.Create(facade.LineGameObject, this.facade.ChatModeSlot.transform);
            }

            this.facade.Button.onClick.AddListener(OnModeSelectClicked);
        }

        public void Initialize()
        {
            facade.ChatModeSlot.SetActive(false);
            facade.ModeText.text = ChatLevel.Say.ToString();

            for (int i = 0; i < config.ChatButtonBGColors.Length; i++)
            {
                if (config.ChatButtonBGColors[i].level != ChatLevel.Say)
                    continue;

                facade.ModeText.color = config.ChatTextButtonColor[i].color;
                facade.Background.color = config.ChatButtonBGColors[i].color;
                break;
            }
        }

        public void LateDispose()
        {
            facade.Button.onClick.RemoveListener(OnModeSelectClicked);
        }

        private void OnModeSelectClicked()
        {
            facade.ChatModeSlot.SetActive(!facade.ChatModeSlot.activeInHierarchy);
        }

        private void OnSelectModeClicked(ChatLevel level)
        {
            facade.ChatModeSlot.SetActive(false);
            facade.ModeText.text = level.ToString();

            for (int i = 0; i < config.ChatButtonBGColors.Length; i++)
            {
                if (config.ChatButtonBGColors[i].level != level)
                    continue;

                facade.ModeText.color = config.ChatTextButtonColor[i].color;
                facade.Background.color = config.ChatButtonBGColors[i].color;
                break;
            }
        }

        public void OnSendLevelChanged(UserChatLevelChangeSignal signal)
        {
            facade.ChatModeSlot.SetActive(false);
            facade.ModeText.text = signal.Level.ToString();

            for (int i = 0; i < config.ChatButtonBGColors.Length; i++)
            {
                if (config.ChatButtonBGColors[i].level != signal.Level)
                    continue;

                facade.ModeText.color = config.ChatTextButtonColor[i].color;
                facade.Background.color = config.ChatButtonBGColors[i].color;
                break;
            }
        }
    }

}