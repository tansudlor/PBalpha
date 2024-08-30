using TMPro;
using System;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatTab : MonoBehaviour
    {
        public string Name => settings.name;
        public ChatLevel[] Filter => settings.filter;

        [Inject]
        private SignalBus signalBus;

        [SerializeField]
        private Image notification;

        [SerializeField]
        private Button mainButton;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private Image bg;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        private short buttonIndex;
        private ChatConfig config;
        private ChatTabSettings settings;
        private Action<int> despawnMethod;

        [Inject]
        private void Construct(ChatConfig config)
        {
            this.config = config;
        }

        public void Initialize(bool ableToRemove, short buttonIndex, ChatTabSettings settings, Action<int> despawnMethod)
        {
            this.settings = settings;
            icon.sprite = settings.icon;
            text.text = this.settings.name;
            this.buttonIndex = buttonIndex;
            this.despawnMethod = despawnMethod;
            notification.gameObject.SetActive(false);
            mainButton.onClick.AddListener(OnMainButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (ableToRemove)
                return;

            closeButton.interactable = false;
            closeButton.gameObject.SetActive(false);
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void SetNotification(bool notify)
        {
            notification.gameObject.SetActive(notify);
        }

        public void SetActive(bool isActive)
        {
            bg.color = isActive ? config.TabActiveBgColor : config.TabDefaultBgColor;
            icon.color = isActive ? config.TabActiveFontColor : config.TabDefaultFontColor;
            text.color = isActive ? config.TabActiveFontColor : config.TabDefaultFontColor;
        }

        public void Dispose()
        {
            mainButton.onClick.RemoveListener(OnMainButtonClicked);
        }

        private void OnDestroy()
        {
            mainButton.onClick.RemoveListener(OnMainButtonClicked);
        }

        private void OnMainButtonClicked()
        {
            signalBus.Fire(new UserTabChangeSignal(buttonIndex));
        }

        private void OnCloseButtonClicked()
        {
            despawnMethod?.Invoke(buttonIndex);
        }

        public class Pool : MonoMemoryPool<bool, short, Transform, ChatTabSettings, Action<int>, ChatTab>
        {
            protected override void Reinitialize(bool ableToRemove, short buttonIndex, Transform parent, ChatTabSettings settings, Action<int> despawnMethod, ChatTab item)
            {
                item.transform.SetParent(parent);
                item.Initialize(ableToRemove, buttonIndex, settings, despawnMethod);
            }

            protected override void OnDespawned(ChatTab item)
            {
                item.Dispose();
                base.OnDespawned(item);
            }
        }
    }
}