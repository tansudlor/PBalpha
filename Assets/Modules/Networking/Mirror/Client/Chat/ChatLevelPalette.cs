using TMPro;
using System;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatLevelPalette : ILateDisposable
    {
        private ChatLevelAndCommand commandAndFullName;
        private Action<ChatLevel> onSelect;

        private readonly ChatConfig config;
        private readonly SignalBus signalBus;
        private readonly ChalLevelPaletteFacade facade;

        public ChatLevelPalette(SignalBus signalBus, ChalLevelPaletteFacade facade, ChatConfig config)
        {
            this.facade = facade;
            this.config = config;
            this.signalBus = signalBus;
        }

        public void Initialize(ChatLevelAndCommand level, Transform parent, Action<ChatLevel> onSelect)
        {
            this.onSelect = onSelect;
            commandAndFullName = level;
            facade.Transform.SetParent(parent);
            facade.CommandText.text = commandAndFullName.command;
            facade.NameText.text = commandAndFullName.level.ToString();
            facade.Button.onClick.AddListener(OnSelect);

            for (int i = 0; i < config.ChatButtonBGColors.Length; i++)
            {
                if (commandAndFullName.level != config.ChatButtonBGColors[i].level)
                    continue;

                facade.CommandText.color = config.MessageColors[i].color;
                break;
            }
        }

        public void LateDispose()
        {
            facade.Button.onClick.RemoveListener(OnSelect);
        }

        private void OnSelect()
        {
            onSelect?.Invoke(commandAndFullName.level);
            signalBus.Fire(new UserChatLevelChangeSignal(commandAndFullName.level));
        }

        public class Factory : PlaceholderFactory<ChatLevelPalette>
        {

        }
    }

    public readonly struct UserTabChangeSignal : IEquatable<UserTabChangeSignal>
    {
        public readonly short Index;

        public UserTabChangeSignal(short index) : this()
        {
            Index = index;
        }
        public bool Equals(UserTabChangeSignal other)
        {
            return Index == other.Index;
        }
        public override bool Equals(object obj)
        {
            return obj is UserTabChangeSignal other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }

    public readonly struct UserChatLevelChangeSignal
    {
        public ChatLevel Level => level;

        private readonly ChatLevel level;
        public UserChatLevelChangeSignal(ChatLevel level)
        {
            this.level = level;
        }
    }
}