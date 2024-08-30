using com.playbux.sfxwrapper;
using System;
using TMPro;
using UnityEngine;

namespace com.playbux.ui.gamemenu
{
    public class EntranceBulidingUIController : MonoBehaviour
    {
        public delegate void OnPlayerEnter();
        public event Action OnClose;
        public OnPlayerEnter OnAccept;

        [SerializeField]
        private GameObject EntranceBulidingPopUp;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        void Start()
        {
            CloseDialog();
        }

        public void CloseDialog()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            EntranceBulidingPopUp.SetActive(false);
            OnClose?.Invoke();
        }

        public void OpenDialog()
        {
            EntranceBulidingPopUp.SetActive(true);
        }

        public void SetDescription(string description)
        {
            descriptionText.text = description;
        }

        public void OnPLayerClick()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            OnAccept?.Invoke();
        }

    }
}