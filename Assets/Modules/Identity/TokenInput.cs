using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;
using com.playbux.events;
using com.playbux.input;
using com.playbux.networking.mirror.message;
using com.playbux.api;
using Mirror;
using System.Collections;

namespace com.playbux.identity
{
    public class TokenInput : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private Button button;
        [SerializeField]
        private TMP_Text descriptionText;
        [SerializeField]
        private Sprite buttonGreenSprite;
        [SerializeField]
        private Sprite textFieldRedSprite;
        [SerializeField]
        private Image inputFieldImage;
        [SerializeField]
        private Image buttonImage;
        [SerializeField]
        private Sprite origianlButtonSprite;
        [SerializeField]
        private Sprite origianlFieldSprite;
        [SerializeField]
        private Image playText;

        private AuthenticationSignal signal;
        private PlayerControls playerControls;
        private SignalBus signalBus;

        [Inject]
        void SetUp(PlayerControls playerControls, SignalBus signalBus)
        {
            this.signalBus = signalBus;
            this.playerControls = playerControls;

        }

        private void Start()
        {
            button.interactable = false;
            playText.color = new Color(1, 1, 1, 0.5f);

        }


        //when client connected to server 
        public void OnConnectedSignalReceive(AuthenticationSignal signal)
        {
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
        }

        public async void Submit()
        {
            button.interactable = false;
            playText.color = new Color(1, 1, 1, 0.5f);
            string passCode = inputField.text;
            string token = await APIServerConnector.GetWhiteListToken(passCode);
            
            if (token == "Passcode not found")
            {
                StartCoroutine(ChangeText());
                return;
            }
            var characterMessage = new AuthenticationMessage(token);
            NetworkClient.Send(characterMessage);
            StartCoroutine(DestoryThis());
            
        }

        IEnumerator DestoryThis()
        {
            yield return new WaitForSecondsRealtime(2f);
            playerControls.Chat.Enable();
            playerControls.UI.Enable();
            Destroy(this.gameObject);
        }

        IEnumerator ChangeText()
        {
            descriptionText.text = "Invalid passcode";
            descriptionText.color = Color.red;
            inputFieldImage.sprite = textFieldRedSprite;
            button.interactable = false;
            buttonImage.sprite = origianlButtonSprite;
            playText.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(3f);
            descriptionText.text = "Character length for passcode is 8 characters";
            descriptionText.color = Color.white;
            inputFieldImage.sprite = origianlFieldSprite;

        }

        public void OnChangeText()
        {
            string passCode = inputField.text;
            button.interactable = false;
            if (passCode.Length != 8)
            {
                buttonImage.sprite = origianlButtonSprite;
                playText.color = new Color(1, 1, 1, 0.5f);
                return;
            }


            string userId = ToNumber(passCode[..6]);
            string verifyNum = ToNumber(passCode[6..]);
            if (ToHash(userId).ToString() == verifyNum)
            {
                button.interactable = true;
                buttonImage.sprite = buttonGreenSprite;
                playText.color = new Color(1, 1, 1, 1);
            }
            

        }

        string ToNumber(string input)
        {
            string number = "";

            foreach (char c in input)
            {
                int current = (c - 'E' + 1);
                number += current.ToString();

            }
            return number;

        }

        int ToHash(string input)
        {
            int sum = 0;
            foreach (char c in input)
            {
                try
                {
                    sum += int.Parse(c.ToString());
                }
                catch 
                { 

                }
            }

            return sum;
        }
    }
}
