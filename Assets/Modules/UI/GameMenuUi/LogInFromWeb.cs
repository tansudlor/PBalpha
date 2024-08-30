using com.playbux.events;
using com.playbux.input;
using com.playbux.api;
using com.playbux.networking.mirror.client.chat;
using com.playbux.networking.mirror.infastructure;
using com.playbux.networking.networkinventory;
using UnityEngine;
using Zenject;
using ImaginationOverflow.UniversalDeepLinking;
using System;
using TMPro;
using ModestTree;
using com.playbux.networking.mirror.message;
using Mirror;
using System.Collections;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using com.playbux.sfxwrapper;
using System.Security.Cryptography;
using com.playbux.analytic;
using UnityEngine.UI;
using UnityEngine.Events;
using com.playbux.firebaseservice;
using com.playbux.networking.mirror.core;

namespace com.playbux.ui.gamemenu
{
    public class LogInFromWeb : MonoBehaviour
    {

        public GameObject NotificationUI;
        public TextMeshProUGUI textNotiBox;
        public GameObject ContinueButton;
        public GameObject ExitButton;
        public GameObject QueueBox;
        public Button NotiButton;
        public Image NotiButtonImage;
        public Image NotiHeaderImage;
        public Sprite TryAgainSprite;
        public Sprite QuitToDeskTopSprite;
        public Sprite DownloadNowSprite;
        public Sprite ConnectionErrorHeader;
        public Sprite DownloadNewVersionHeader;

        private AuthenticationSignal signal;
        private PlayerControls playerControls;
        private NetworkController networkController;
        private ChatUIController chatUIController;
        private InventoryUIController inventoryUIController;
        private OpenAnimation openAnimation;
        private SignalBus signalBus;
        private static Coroutine startLogin = null;

        private TransportProvider transportProvider;

        private string logInURL = "";

        private string token;
        private string refresh;

        private int curQueue = 0;
        private int preQueue = 999;

#if !LOGIN_BYPASS
        [Inject]
        void SetUp(PlayerControls playerControls, SignalBus signalBus, NetworkController networkController, InventoryUIController inventoryUIController, ChatUIController chatUIController, OpenAnimation openAnimation
           , TransportProvider transportProvider)
        {
            this.signalBus = signalBus;
            this.playerControls = playerControls;
            this.networkController = networkController;
            this.chatUIController = chatUIController;
            this.inventoryUIController = inventoryUIController;
            this.openAnimation = openAnimation;
            this.transportProvider = transportProvider; 
            //this.queueChecking = queueChecking;
        }

        private void Start()
        {
            logInURL = APIServerConnector.logInURL;
            DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
            var currentTransform = transform.parent;
            transform.SetParent(Camera.main.transform);
            transform.SetParent(currentTransform);
            gameObject.SetActive(false);
        }



        private void Instance_LinkActivated(LinkActivation linkActivation)
        {

            //URL.text = linkActivation.Uri;

            var rawQuery = linkActivation.RawQueryString;

            var rawQuerySplit = rawQuery.Split("&");

            token = rawQuerySplit[0].Split("=")[1];

            refresh = rawQuerySplit[1].Split("=")[1];

            TokenUtility.SetToken(token, refresh);

            SFXWrapper.getInstance().PlaySFX("SFX/Success");
            //AuthenticateToGameServer(token).Forget();

        }

        public async void ShowLogInPage()
        {
            gameObject.SetActive(true);
            if(startLogin != null)
            {
                StopCoroutine(startLogin);
            }
            startLogin = StartCoroutine(StartCheckLogin());
            var currentTransform = transform.parent;
            transform.SetParent(Camera.main.transform);
            transform.SetParent(currentTransform);
#if !SERVER && PRODUCTION
            int lastVersionLocal = int.Parse(Application.version.Split('.')[3]);
            int lastVersionConfig = int.Parse(APIServerConnector.gameClientVersion.Split('.')[3]);
            Debug.Log(lastVersionLocal + ": local  Config : "  +lastVersionConfig);
            if (lastVersionLocal < lastVersionConfig)
            {
                NotiHeaderImage.sprite = DownloadNewVersionHeader;
                NotiButtonImage.sprite = DownloadNowSprite;
                ShowNotiPage("Please download the latest version \r\nto update your game.", "", GameDownloadPage);
                return;
            }
#endif
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
            
            var token = TokenUtility.GetToken();

            if (string.IsNullOrEmpty(token.access))
            {
                if (!string.IsNullOrEmpty(token.refresh))
                {
                    var tokenResponse = await TokenUtility.GetToken(token.refresh);

                    if (string.IsNullOrEmpty(tokenResponse.access) && string.IsNullOrEmpty(tokenResponse.refresh))
                    {

                        return;
                    }

                    TokenUtility.SetNewAccessTokenData(tokenResponse.access);
                    //Debug.Log("SetNewAccessTokenData");
                    await AuthenticateToGameServer(tokenResponse.access);
                    return;
                }

                return;

            }

            string accessToken = token.access;

            await AuthenticateToGameServer(accessToken);
            //Debug.Log("AuthenticateToGameServer");
            return;


        }

        public void ShowNotiPage(string desc, string accessToken, UnityAction action)
        {
            SFXWrapper.getInstance().PlaySFX("SFX/ErrorBox");
            NotificationUI.SetActive(true);
            textNotiBox.text = desc;
            PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
            Logout(accessToken).Forget();
            NotiButton.onClick.RemoveAllListeners();
            NotiButton.onClick.AddListener(action);
        }

        public void GameDownloadPage()
        {
            string playbuxPage = "https://alpha.playbux.co/";
            Application.OpenURL(playbuxPage);
            
        }

        public void CloseNotiBox()
        {
            NotificationUI.SetActive(false);
            ContinueButton.SetActive(true);
            ExitButton.SetActive(true);
            openAnimation.PlaybuxLogoLogInSkeleton.gameObject.SetActive(true);
        }

        public void LogInButton()
        {
            logInURL = APIServerConnector.logInURL;
            //Debug.Log(logInURL);
            Application.OpenURL(logInURL);
           
        }



        IEnumerator StartCheckLogin()
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(PlayerPrefs.GetString(TokenUtility.accessTokenKey)));
            SFXWrapper.getInstance().PlaySFX("SFX/Success");
            AuthenticateToGameServer(PlayerPrefs.GetString(TokenUtility.accessTokenKey)).Forget();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void OnConnectedSignalReceive(AuthenticationSignal signal)
        {
            playerControls.Chat.Disable();
            playerControls.UI.Disable();
        }



        public async UniTask AuthenticateToGameServer(string accessToken)
        {
            gameObject.SetActive(true);
            ContinueButton.SetActive(false);
            ExitButton.SetActive(false);
            openAnimation.PlaybuxLogoLogInSkeleton.gameObject.SetActive(false);
            openAnimation.ClickToLogInSkeleton.gameObject.SetActive(false);
            while (true)
            {
                GameMenuUiController.isQueue = true;
                
                string status = await CheckQueue(accessToken);

                if (status == "connecting")
                {
                    QueueBox.SetActive(false);
                    //Debug.Log("[QUEUE] : connecting and play");
                    break;
                }
                else if (status == "playing")
                {
                    QueueBox.SetActive(false);
                    NotiHeaderImage.sprite = ConnectionErrorHeader;
                    NotiButtonImage.sprite = QuitToDeskTopSprite;
                    ShowNotiPage("You've already played this game", accessToken, QuitGame);
                    return;
                }
                else if (status == "waiting")
                {
                    if (curQueue >= preQueue)
                    {
                        curQueue = preQueue;
                    }

                    QueueBox.GetComponent<QueueChecking>().WaitNumber.text = curQueue.ToString();

                    QueueBox.SetActive(true);
                    preQueue = curQueue;
                }
                else
                {
                    //fix here queue error
                    QueueBox.SetActive(false);
                    NotiHeaderImage.sprite = ConnectionErrorHeader;
                    NotiButtonImage.sprite = TryAgainSprite;
                    ShowNotiPage("High demand on the access queue.\r\nPlease try again.", accessToken, CloseNotiBox);
                    return;
                }

                await UniTask.WaitForSeconds(60f);
            }

            GameMenuUiController.isQueue = false;
#if REMOTE_ENDPOINT
            FirebaseRemoteConfigManager.GetInstance();
            await UniTask.WaitUntil(() => !string.IsNullOrEmpty(FirebaseRemoteConfigManager.GetInstance().EndPoint));
            networkController.Manager.networkAddress = FirebaseRemoteConfigManager.GetInstance().EndPoint;
            networkController.Manager.transport = transportProvider.Get("default");
#endif
            networkController.Manager.StartClient();
            signalBus.Fire(new LoginSignal());
            await UniTask.WaitUntil(() => NetworkClient.ready);
            AuthenticateToken(accessToken);
        }

        public async void GamePage(string accessToken) //
        {

            SFXWrapper.getInstance().PlaySFX("SFX/Success");
            networkController.Manager.StartClient();
            signalBus.Fire(new LoginSignal());
            await UniTask.WaitUntil(() => NetworkClient.ready);
            AuthenticateToken(accessToken);
        }


        void AuthenticateToken(string token)
        {
            try
            {
                AnalyticWrapper.getInstance().Log("login_success",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     );
            }
            catch
            {

            }
            var characterMessage = new AuthenticationMessage(token);
            NetworkClient.Send(characterMessage);
            chatUIController.Close();
            inventoryUIController.Close();
            this.gameObject.SetActive(false);
            openAnimation.CloseThisAnim();

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

        async UniTask<string> Logout(string accessToken)
        {
            string logout = await APIServerConnector.LogOutAPI(accessToken);

            if (logout != null)
            {
                return logout;
            }
            return logout;
        }

        public async UniTask<string> CheckQueue(string accessToken)
        {
            JObject queueData = await APIServerConnector.QueueMe(accessToken);
            string status = "";
            try
            {
                status = queueData["data"]["status"].ToString();
                curQueue = queueData["data"]["waiting"].ToObject<int>() + 1;
            }
            catch (Exception e)
            {
                status = e.Message;
            }

            return status;
        }


#endif
    }
}
