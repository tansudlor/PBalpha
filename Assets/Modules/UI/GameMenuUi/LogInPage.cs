
using com.playbux.api;
using com.playbux.events;
using com.playbux.input;
using com.playbux.networking.mirror.message;
using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using com.playbux.networking.mirror.infastructure;
using com.playbux.networking.networkinventory;
using com.playbux.networking.mirror.client.chat;
using System.Security.Policy;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using com.playbux.settings;
using System.IO;
using UnityEngine.EventSystems;
using com.playbux.firebaseservice;
using com.playbux.networking.mirror.core;


namespace com.playbux.ui.gamemenu
{
    public class LogInPage : MonoBehaviour //BYPASSMODE
    {

        [SerializeField]
        private GameObject InvalidTextGameobject;

        [SerializeField]
        private TMP_InputField passwordInputField;
        [SerializeField]
        private TMP_InputField emailInputField;

        [SerializeField]
        private TMP_Text errorText;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Image playButtonImage;
        [SerializeField]
        private Image playTextImage;
        [SerializeField]
        private Image eyeImage;
        [SerializeField]
        private Image passwordInputFieldImage;
        [SerializeField]
        private Image emailInputFieldImage;

        [SerializeField]
        private Sprite eyeClose;
        [SerializeField]
        private Sprite eyeOpen;
        [SerializeField]
        private Sprite buttonGreenSprite;
        [SerializeField]
        private Sprite buttonGraySprite;
        [SerializeField]
        private Sprite fieldGraySprite;
        [SerializeField]
        private Sprite fieldRedSprite;


        public GameObject NotificationUI;
        public TextMeshProUGUI textNotiBox;

        private AuthenticationSignal signal;
        private PlayerControls playerControls;
        private NetworkController networkController;
        private ChatUIController chatUIController;
        private InventoryUIController inventoryUIController;
        private SettingDataBase settingDataBase;
        private SignalBus signalBus;
        private TransportProvider transportProvider;
        private PlaybuxNetworkManager playbuxNetworkManager;
        private bool isOn = false;
        private bool isSubmit = false;
#if LOGIN_BYPASS
        [Inject]
        void SetUp(PlayerControls playerControls, SignalBus signalBus, NetworkController networkController,InventoryUIController inventoryUIController,ChatUIController chatUIController, SettingDataBase settingDataBase, TransportProvider transportProvider
            ,PlaybuxNetworkManager playbuxNetworkManager)
        {
            this.signalBus = signalBus;
            this.playerControls = playerControls;
            this.networkController = networkController;
            this.chatUIController = chatUIController;
            this.inventoryUIController = inventoryUIController;
            this.settingDataBase = settingDataBase;
            this.transportProvider = transportProvider;
            this.playbuxNetworkManager = playbuxNetworkManager;
        }
        // Start is called before the first frame update
        async void Start()
        {
           
            ShowLogInPage();

            var token = TokenUtility.GetToken();

            if (string.IsNullOrEmpty(token.access))
            {
                if (!string.IsNullOrEmpty(token.refresh))
                {
                    var tokenResponse = await TokenUtility.GetToken(token.refresh);

                    if(string.IsNullOrEmpty(tokenResponse.access) && string.IsNullOrEmpty(tokenResponse.refresh))
                    {
                        
                        return;
                    }

                    TokenUtility.SetNewAccessTokenData(tokenResponse.access);
                    await AuthenticateToGameServer(tokenResponse.access);
                    return;
                }

                return;

            }

            string accessToken = token.access;

            await AuthenticateToGameServer(accessToken);
            return;

        }

        private void Update()
        {

            string email = emailInputField.text;
            string password = passwordInputField.text;

            playButton.interactable = false;
            playTextImage.color = new Color(1, 1, 1, 0.5f);
            playButtonImage.sprite = buttonGraySprite;

            if (!email.Contains("@") && !email.Contains("."))
            {
                return;
            }

            if (email.Length < 5)
            {
                return;
            }

            if (string.IsNullOrEmpty(password))
            {

                return;
            }

            if (password.Length <= 0)
            {
                return;
            }

            if (isSubmit == false)
            {
                playButton.interactable = true;
                playTextImage.color = new Color(1, 1, 1, 1);
                playButtonImage.sprite = buttonGreenSprite;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Submit();
            }

        }

        public void OnConnectedSignalReceive(AuthenticationSignal signal)
        {
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
        }

        public async void Submit()
        {
            isSubmit = true;
            playButton.interactable = false;
            playTextImage.color = new Color(1, 1, 1, 0.5f);
            playButtonImage.sprite = buttonGraySprite;

            string email = emailInputField.text;
            string password = passwordInputField.text;

            var token = await TokenUtility.GetToken(email, password);

            if (string.IsNullOrEmpty(token.access) && string.IsNullOrEmpty(token.refresh))
            {
                StartCoroutine(ChangeText());
                return;

            }

            string accessToken = token.access;

            await AuthenticateToGameServer(accessToken);

        }


        async UniTask AuthenticateToGameServer(string accessToken)
        {
            signalBus.Fire(new LoginSignal());
            bool isBackendDown = await CheckAPI(accessToken);
            Debug.Log("isBackendDown" + isBackendDown);
            if (isBackendDown)
            {

                //Authenticate Error
                NotificationUI.SetActive(true);
                textNotiBox.text = "Authentication Fail";
                PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
                PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
                return;

            }

#if REMOTE_ENDPOINT
            FirebaseRemoteConfigManager.GetInstance();
            await UniTask.WaitUntil(() => !string.IsNullOrEmpty(FirebaseRemoteConfigManager.GetInstance().EndPoint));
            networkController.Manager.networkAddress = FirebaseRemoteConfigManager.GetInstance().EndPoint;
            networkController.Manager.transport = transportProvider.Get("default");
#endif
            networkController.Manager.StartClient();
            await UniTask.WaitUntil(() => NetworkClient.ready);
            AuthenticateToken(accessToken);
        }

        void AuthenticateToken(string token)
        {
            var characterMessage = new AuthenticationMessage(token);
            NetworkClient.Send(characterMessage);
            chatUIController.Close();
            inventoryUIController.Close();
            this.gameObject.SetActive(false);
        }


        IEnumerator ChangeText()
        {
            InvalidTextGameobject.SetActive(true);
            passwordInputFieldImage.sprite = fieldRedSprite;
            emailInputFieldImage.sprite = fieldRedSprite;
            playButton.interactable = false;
            playButtonImage.sprite = buttonGraySprite;
            playTextImage.color = new Color(1, 1, 1, 0.5f);
            passwordInputField.text = "";
            yield return new WaitForSeconds(3f);

            InvalidTextGameobject.SetActive(false);
            passwordInputFieldImage.sprite = fieldGraySprite;
            emailInputFieldImage.sprite = fieldGraySprite;
            isSubmit = false;

        }

        public void ShowPassword()
        {
            isOn = !isOn;

            if (isOn == false)
            {
                passwordInputField.contentType = TMP_InputField.ContentType.Password;
                eyeImage.sprite = eyeClose;
            }
            else
            {
                passwordInputField.contentType = TMP_InputField.ContentType.Standard;
                eyeImage.sprite = eyeOpen;
            }
            passwordInputField.ForceLabelUpdate();
        }

        public void ShowLogInPage()
        {
            this.gameObject.SetActive(true);
            playButton.interactable = false;
            playTextImage.color = new Color(1, 1, 1, 0.5f);
            passwordInputField.asteriskChar = '●';
            emailInputField.text = "";
            passwordInputField.text = "";
            isSubmit = false;
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
           
        }


        public void QutiGame()
        {
            Application.Quit();
        }

        async UniTask<bool> CheckAPI(string accessToken)
        {
            UserProfile userProfile = await APIServerConnector.GetMe(accessToken);

            if (userProfile == null)
            {
                return true;
            }
            return false;
        }

#endif
    }
}
