using System;
using UnityEngine;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    [CreateAssetMenu(menuName = "Playbux/UI/Create ChatConfig", fileName = "ChatConfig", order = 0)]
    public class ChatConfig : ScriptableObject
    {
        public Color TabActiveBgColor => tabActiveBGColor;
        public Color TabActiveFontColor => tabActiveFontColor;
        public Color TabDefaultBgColor => tabDefaultBGColor;
        public Color TabDefaultFontColor => tabDefaultFontColor;
        public Color TabNotificationColor => tabNotificationColor;
        public MessageColor[] MessageColors => messageColors;
        public ChatLevelAndCommand[] Levels => chatLevelAndCommands;
        public ChatIconColor[] ChatButtonBGColors => chatButtonBGColors;
        public ChatTextButtonColor[] ChatTextButtonColor => chatTextButtonColors;

        [SerializeField]
        private Color tabActiveBGColor;

        [SerializeField]
        private Color tabActiveFontColor;

        [SerializeField]
        private Color tabDefaultBGColor;

        [SerializeField]
        private Color tabDefaultFontColor;

        [SerializeField]
        private Color tabNotificationColor;

        [SerializeField]
        private MessageColor[] messageColors;

        [SerializeField]
        private ChatIconColor[] chatButtonBGColors;

        [SerializeField]
        private ChatTextButtonColor[] chatTextButtonColors;

        [SerializeField]
        private ChatLevelAndCommand[] chatLevelAndCommands;
    }

    [Serializable]
    public class MessageColor
    {
        public ChatLevel level;
        public Color color;
    }

    [Serializable]
    public class ChatIconColor
    {
        public ChatLevel level;
        public Color color;
    }

    [Serializable]
    public class ChatTextButtonColor
    {
        public ChatLevel level;
        public Color color;
    }
}