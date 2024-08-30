using com.playbux.events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using com.playbux.api;

namespace com.playbux.ui.gamemenu
{

    public class NotificationUIController : MonoBehaviour
    {
        [SerializeField]
        private Image headerImage;
        [SerializeField]
        private Image buttonImage;
        [SerializeField]
        private Button buttonOnClick;
        [SerializeField]
        private Sprite[] headerSprites;
        [SerializeField]
        private Sprite[] buttonSprites;
        [SerializeField]
        private TextMeshProUGUI description;

        private SignalBus signalBus;

        [Inject]
        void SetUp(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            this.signalBus.Subscribe<NotificationUISignal>(OnNotificationRecieve);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public NotificationType type;

        public void TestButton()
        {
            SetNotiTypeAndDescription(type, "Your account has been logged in \r\non other devices right now. \r\nPlease update your password.");
        }
#endif
        public void SetNotiTypeAndDescription(NotificationType notiType, string desc)
        {
            headerImage.sprite = headerSprites[(int)notiType];
            buttonImage.sprite = buttonSprites[(int)notiType];
            description.text = desc;
        }

        public void SetButtonClick(UnityAction action)
        {
            buttonOnClick.onClick.RemoveAllListeners();
            buttonOnClick.onClick.AddListener(action);
        }

        public void QuitGame()
        {
#if !UNITY_EDITOR
            PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
#endif
            Application.Quit();
        }

        public void OnNotificationRecieve(NotificationUISignal signal)
        {
           
            int type = signal.type;
            string desc = signal.desc;
            SetNotiTypeAndDescription((NotificationType)type, desc);
            var currectParent = gameObject.transform.parent;
            gameObject.transform.SetParent(Camera.main.transform);
            gameObject.transform.SetParent(currectParent);
            gameObject.SetActive(true);
        }


    }
}
