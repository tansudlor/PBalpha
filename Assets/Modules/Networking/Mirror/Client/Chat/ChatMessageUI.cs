using TMPro;
using Zenject;
using UnityEngine;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatMessageUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private Dictionary<ChatLevel, Color> messageColors;

        [Inject]
        private void Construct(Dictionary<ChatLevel, Color> messageColors)
        {
            this.messageColors = messageColors;
        }

        public void Initialize(string message, ChatLevel chatLevel)
        {
            text.text = message;
            text.color = messageColors[chatLevel];
        }

        public void Dispose()
        {
            text.text = "";
        }

        public class Pool : MonoMemoryPool<string, ChatLevel, Transform, ChatMessageUI>
        {
            protected override void Reinitialize(string message, ChatLevel chatLevel, Transform parent, ChatMessageUI messageUI)
            {
                messageUI.transform.SetParent(parent);
                messageUI.Initialize(message, chatLevel);
            }

            protected override void OnDespawned(ChatMessageUI item)
            {
                base.OnDespawned(item);
                item.Dispose();
            }
        }
    }

}