using TMPro;
using Mirror;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.input;
using com.playbux.events;
using System.Collections;
using com.playbux.sfxwrapper;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;

namespace com.playbux.ui.gamemenu
{

    public class ChangeNameUIController : MonoBehaviour, IChangeNameUIController
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TMP_Text descriptionText;
        [SerializeField]
        private Button yesButton;
        [SerializeField]
        private Sprite buttonGreenSprite;
        [SerializeField]
        private Sprite origianlButtonSprite;
        [SerializeField]
        private Sprite textFieldRedSprite;
        [SerializeField]
        private Sprite origianlFieldSprite;
        [SerializeField]
        private Image inputFieldImage;
        [SerializeField]
        private Image yesText;
        [SerializeField]
        private Image buttonImage;


        private SignalBus signalBus;
        private PlayerControls playerControls;
        private IdentityChangeSignal changeSignal;

        [Inject]
        private void SetUp(SignalBus signalBus,PlayerControls playerControls)
        {
            this.signalBus = signalBus;
            this.playerControls = playerControls;
        }

        private void Start()
        {
            this.gameObject.SetActive(false);
            yesButton.interactable = false;
            yesText.color = new Color(1, 1, 1, 0.5f);
        }

        public void Submit()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/MessageBox");
            yesButton.interactable = false;
            string name = inputField.text;
            IdentityChangeSignal changeNameSignal = new IdentityChangeSignal();
            changeNameSignal.Command = "name";
            changeNameSignal.Data = name;
            signalBus.Fire(changeNameSignal);
            Hide();

        }

        public void Show()
        {

            this.gameObject.SetActive(true);
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
            playerControls.Movement.Disable();
        }

        public void Hide()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            playerControls.Chat.Enable();
            playerControls.UI.Enable();
            playerControls.Movement.Enable();
            this.gameObject.SetActive(false);

        }

        public void OnChangeText()
        {
            string nameLength = inputField.text;
            yesButton.interactable = false;
            if (nameLength.Length > 10)
            {
                descriptionText.text = "Character length exceeds 10 characters.";
                descriptionText.color = Color.red;
                inputFieldImage.sprite = textFieldRedSprite;
                buttonImage.sprite = origianlFieldSprite;
                yesText.color = new Color(1, 1, 1, 0.5f);
                return;
            }
            descriptionText.text = "The maximum character length for a name is 10 characters.";
            descriptionText.color = Color.white;
            yesButton.interactable = true;
            inputFieldImage.sprite = origianlFieldSprite;
            buttonImage.sprite = buttonGreenSprite;
            yesText.color = new Color(1, 1, 1, 1);

        }
    }
}