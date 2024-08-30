using System.Collections;
using UnityEngine;
using com.playbux.identity;
using TMPro;
using Zenject;
using Mirror;
using com.playbux.input;
using com.playbux.networkquest;
using com.playbux.networking.networkinventory;
using com.playbux.events;
using System;
using com.playbux.api;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using com.playbux.minimap;
using com.playbux.networking.mirror.message;
using com.playbux.flag;
using com.playbux.quest;
using UnityEngine.Events;
using com.playbux.firebaseservice;
using com.playbux.tool;
using Newtonsoft.Json.Linq;
using System.Linq;
using com.playbux.sfxwrapper;
using com.playbux.analytic;
using com.playbux.ui.leaderboard;

namespace com.playbux.ui.gamemenu
{
    //FIXME: this class has became god's object already, it's bloated. need to decouple many dependencies of this class into its own module. definitely need to refactor
    public class GameMenuUiController : MonoBehaviour, IGameMenuUiController
    {


        public static bool isQueue = false;

        //FIXME: these too are dependencies
        [SerializeField]
        private TextMeshProUGUI[] playerName;
        [SerializeField]
        private GameObject questComplete;
        [SerializeField]
        private GameObject miniMap;
        [SerializeField]
        private GameObject LeftUI;
        [SerializeField]
        private GameObject rightUI;
        [SerializeField]
        private RectTransform rect;
        [SerializeField]
        private Animator welcomePlayer;
        [SerializeField]
        private TextMeshProUGUI currentBrk;
        [SerializeField]
        private TextMeshProUGUI currentLotto;
        [SerializeField]
        private TextMeshProUGUI currentPebble;
        [SerializeField]
        private DialogLinkOutData tutorialDialogData;
        [SerializeField]
        private DialogLinkOutData shopToEarnDialogData;
        [SerializeField]
        private DialogLinkOutData playbuxShopDialogData;
        [SerializeField]
        private GameObject blackDrop;

        public PlaybuxNews playbuxNews;

#if !SERVER
        private IChangeNameUIController changeNameUIController;
        private IIdentitySystem identitySystem;
        private ISettingUIController settingUIController;
        private IFlagCollection<string> flagCollection;
        private IQuestRunner questRunner;
        private PlayerControls playerControls;
#if PRODUCTION
        private long fifteenMin = 60 * 15;
#endif
#if !PRODUCTION
        private long fifteenMin = 60 * 1;
#endif
#if LOGIN_BYPASS
        private LogInPage loginPage;
#endif
#if !LOGIN_BYPASS
        private LogInFromWeb logInFromWeb;
#endif
        private QuestHelperWindow questHelperWindow;
        private InventoryUIController inventoryUIController;
        private SignalBus signalBus;
        private string uid;
        private string playName;
        private string refreshToken;
        private DateTime refreshTokenExpireTime;
        private string accessToken;
        private DateTime accessTokenExpireTime;
        private Coroutine tokenTimer;
        private Coroutine waitToSyncTimer;
        [SerializeField]
        private MiniMapLocator miniMapLocator;
        private FullMiniMapController fullMiniMapController;
        private ClientSettingServiceNetwork clientSettingServiceNetwork;
        private DialogTempleteController dialogTempleteController;
        private LeaderBoardUI leaderBoardUI;

        private bool isOpenFullMiniMap = true;

        //FIXME: an example of too many dependencies
        //FIXME: always explicit 'private' keyword on a private method please
        private bool isWalletActive = false;

        private long serverTicks = 0;
       

        [Inject]
        void SetUp(
            SignalBus signalBus,

#if LOGIN_BYPASS
            LogInPage loginPage,
#endif
#if !LOGIN_BYPASS
            LogInFromWeb logInFromWeb,
#endif

            PlayerControls playerControls,
            IIdentitySystem identitySystem,
            QuestHelperWindow questHelperWindow,
            ISettingUIController settingUIController,
            InventoryUIController inventoryUIController,
            FullMiniMapController fullMiniMapController,
            DialogTempleteController dialogTempleteController,
            IChangeNameUIController changeNameUIController,
            IFlagCollection<string> flagCollectionBase,
            IQuestRunner questRunner,
            LeaderBoardUI leaderBoardUI)

        {

#if LOGIN_BYPASS
            this.loginPage = loginPage;
#endif
#if !LOGIN_BYPASS
            this.logInFromWeb = logInFromWeb;
#endif
            this.signalBus = signalBus;
            this.identitySystem = identitySystem;
            this.playerControls = playerControls;
            this.questHelperWindow = questHelperWindow;
            this.settingUIController = settingUIController;
            this.inventoryUIController = inventoryUIController;
            this.changeNameUIController = changeNameUIController;
            this.fullMiniMapController = fullMiniMapController;
            this.flagCollection = flagCollectionBase;
            this.questRunner = questRunner;
            this.dialogTempleteController = dialogTempleteController;
            this.leaderBoardUI = leaderBoardUI;
            this.signalBus.Subscribe<QuestRewardSignal>(OnRewardRecieve);
            this.signalBus.Subscribe<ChangeThisPlayerNameSignal>(OnChangeNameRecieve);
            this.signalBus.Subscribe<LinkOutSignal>(OnLinkOutSignalRecieve);
            this.signalBus.Subscribe<RemoteConfigResponseSignal<string>>(OnRemoteConfigResponseSignal);
        }

        private void Start()
        {
            blackDrop.SetActive(true);
            string _id = null;

            try
            {
                _id = PlayerPrefs.GetString(TokenUtility._id);
            }
            catch
            {

            }

            AnalyticWrapper.getInstance().Log("open_application",
            new LogParameter("user_id", _id)
            );

            //clientSettingServiceNetwork = ClientSettingServiceNetwork.GetInstance();
            //clientSettingServiceNetwork.OnSettingDataReceive += ShowDialogWithData;

            fullMiniMapController.MiniMapLocator.RefMiniMapData = miniMapLocator;
            questRunner.FunctionCall.Add("shop_to_earn", ShowShopToEarn);
            questRunner.FunctionCall.Add("playbux_shop", ShowPlaybuxShop);

            ShowGameMenu();

            //client
#if PRODUCTION
            signalBus.Fire(new RemoteConfigFetchRequestSignal("service-uid", 0));
            signalBus.Fire(new RemoteConfigFetchRequestSignal("game-client-version", 0));
#endif
        }

        public void OnRemoteConfigResponseSignal(RemoteConfigResponseSignal<string> signal)
        {
#if PRODUCTION
            if (signal.key == "service-uid")
            {
                APIServerConnector.serviceUid = signal.value;
            }
            else if(signal.key == "game-client-version")
            {
                APIServerConnector.gameClientVersion = signal.value;
                Debug.Log(signal.value);
            }
#endif
        }

        public void ShowShopToEarn()
        {

            dialogTempleteController.SetData(shopToEarnDialogData);
            dialogTempleteController.OpenDialogTemplate();
        }
        public void ShowPlaybuxShop()
        {
            dialogTempleteController.SetData(playbuxShopDialogData);
            dialogTempleteController.OpenDialogTemplate();

        }

        /* private void ShowDialogWithData(string path,JObject data,bool success)
         {
             Debug.Log("ShowDialogWithData : path" + path);
             Debug.Log("ShowDialogWithData : data" + JsonConvert.SerializeObject(data));
             Debug.Log("ShowDialogWithData : success" + success);
             var actionSetting = data["action"];
             Debug.Log(actionSetting["spcae"]["bottom"].ToObject<int>());
             Debug.Log(actionSetting["spcae"]["top"].ToObject<int>());
             Debug.Log(actionSetting["type"].ToString());
             var contentSetting = data["content"];
             Debug.Log(contentSetting.Children().Count());
             for (int i = 0; i < contentSetting.Children().Count(); i++)
             {
                 Debug.Log(contentSetting[i].ToString());
             }
             var desc = data["desc"].ToString();
             Debug.Log(desc);
             var size = data["size"].ToString();
             Debug.Log(size);
             var title = data["title"].ToString();
             Debug.Log(title);
         }*/

        private void Update()
        {


            /*if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                SFXWrapper.getInstance().Profile = 1;
            }
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                SFXWrapper.getInstance().Profile = 2;
            }
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                SFXWrapper.getInstance().Profile = 3;
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                SFXWrapper.getInstance().Profile = 4;
            }*/


            if (!isWalletActive)
            {
                return;
            }

            if (NetworkClient.localPlayer == null)
            {
                return;
            }

            if (NetworkClient.localPlayer.netId == 0)
            {

                return;
            }

            int brk = identitySystem[NetworkClient.localPlayer.netId].BalanceBrk;
            int lotto = identitySystem[NetworkClient.localPlayer.netId].BalanceLottoTickets;
            int pebble = identitySystem[NetworkClient.localPlayer.netId].BalancePebble;

            currentBrk.text = brk.ToString();
            currentLotto.text = lotto.ToString();
            currentPebble.text = pebble.ToString();

        }

        public void ShowGameMenu()
        {
            refreshTokenExpireTime = TokenUtility.GetExpiryDate(TokenUtility.expiryRefeshTokenDateKey);
            refreshToken = PlayerPrefs.GetString(TokenUtility.refreshTokenKey);
            accessTokenExpireTime = TokenUtility.GetExpiryDate(TokenUtility.expiryAccessTokenDateKey);
            accessToken = PlayerPrefs.GetString(TokenUtility.accessTokenKey);

            welcomePlayer.speed = 0;
            LeftUI.transform.localScale = Vector3.zero;
            rightUI.transform.localScale = Vector3.zero;
            questHelperWindow.CloseQuestHelper();
            changeNameUIController.Hide();

            miniMap.SetActive(false);
            questComplete.SetActive(false);

            if (waitToSyncTimer != null)
            {
                waitToSyncTimer = null;
            }
            if (tokenTimer != null)
            {
                tokenTimer = null;
            }
            waitToSyncTimer = StartCoroutine(WaitForSync());
            tokenTimer = StartCoroutine(TokenCheck());
            playerControls.Chat.Enable();
            playerControls.UI.Enable();
        }

        public void PlayerChangeID()
        {
            StopCoroutine(waitToSyncTimer);
            StopCoroutine(tokenTimer);
        }

        public void OnRewardRecieve(QuestRewardSignal signal)
        {
            UserBalance userBalance = (UserBalance)signal.Data;
            identitySystem[NetworkClient.localPlayer.netId].BalanceBrk = userBalance.Brk;
            identitySystem[NetworkClient.localPlayer.netId].BalanceLottoTickets = userBalance.Lotto;
            identitySystem[NetworkClient.localPlayer.netId].BalancePebble = userBalance.Pebble;

        }


        public void OnChangeNameRecieve(ChangeThisPlayerNameSignal signal)
        {
            string name = signal.ThisNameChange;
            ShowDisplayName(name);
        }

        public void OpenChangeNameDialog()
        {

            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "change_name")
                     );
            }
            catch
            {

            }


            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            changeNameUIController.Show();
        }
        public void OpenQuestHelper()
        {
            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "quest_helper")
                     );
            }
            catch
            {

            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            questHelperWindow.OpenQuestHelper();

        }

        public void OpenSetting()
        {
            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "setting")
                     );
            }
            catch
            {

            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            settingUIController.OpenSetting();
        }

        public void ToggleInventory()
        {
            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "inventory")
                     );
            }
            catch
            {

            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            inventoryUIController.Toggle();
        }

        public void FullMiniMapToggle()
        {
            if (isOpenFullMiniMap)
            {
                fullMiniMapController.Show();
                isOpenFullMiniMap = false;
            }
            else
            {
                fullMiniMapController.Hide();
                isOpenFullMiniMap = true;
            }
        }

        public void ToggleLeaderBoard()
        {
            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "leaderboard")
                     );
            }
            catch
            {

            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");

            leaderBoardUI.OpenLeaderBoard();
        }

        IEnumerator WaitForSync() //FIXME: always explicit 'private' keyword on a private method please
        {
            //Debug.Log("WaitForSync()");
            yield return new WaitWhile(() => NetworkClient.localPlayer == null);
            //Debug.Log("NetworkClient.localPlayer != null");
            yield return new WaitUntil(() => identitySystem.ContainsKey(NetworkClient.localPlayer.netId));
            //Debug.Log("identitySystem.ContainsKey(NetworkClient.localPlayer.netId) : " + identitySystem.ContainsKey(NetworkClient.localPlayer.netId));
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId] != null);
            //Debug.Log("identitySystem[NetworkClient.localPlayer.netId] : " + identitySystem[NetworkClient.localPlayer.netId]);
            Destroy(blackDrop);


            //FIXME: LoginTime must from server

            bool wait = true;
            while (wait)
            {
                try
                {
                    wait = identitySystem[NetworkClient.localPlayer.netId].DisplayName == null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Server Disconnect Current Player: " + e.Message);
                    //show discoonect Dialog
                    yield break;
                }
                yield return null;
            }

            //signalBus.Fire(new BGMStopSignal("BGM/Login"));
            //yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId].DisplayName != null);

            playName = identitySystem[NetworkClient.localPlayer.netId].DisplayName;
            uid = identitySystem[NetworkClient.localPlayer.netId].ID;
            serverTicks = identitySystem[NetworkClient.localPlayer.netId].LoginTime.Ticks;
            TickCounter().Forget();
            Debug.Log("Pebble : " + identitySystem[NetworkClient.localPlayer.netId].BalancePebble);

            int brk = identitySystem[NetworkClient.localPlayer.netId].BalanceBrk;
            int lotto = identitySystem[NetworkClient.localPlayer.netId].BalanceLottoTickets;
            int pebble = identitySystem[NetworkClient.localPlayer.netId].BalancePebble;

            ShowDisplayName(playName);

#if LOGIN_BYPASS
            yield return new WaitUntil(() => !loginPage.gameObject.activeInHierarchy);
#endif
#if !LOGIN_BYPASS
            yield return new WaitUntil(() => !logInFromWeb.gameObject.activeInHierarchy);
#endif
            welcomePlayer.Play("welcomeAnimator", 0, 0);
            welcomePlayer.speed = 1f;

            yield return null;
            yield return new WaitUntil(() => welcomePlayer.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);

            StartCoroutine(ScaleTo1OverTime());
            OpenQuestHelper();
            Destroy(welcomePlayer.gameObject);
            if (playName == uid)
            {
                ShowDisplayName("NewBux" + NetworkClient.localPlayer.netId);
            }

            isWalletActive = true;

        }

        private IEnumerator ScaleTo1OverTime()
        {
            LeftUI.transform.localScale = Vector3.one;
            rightUI.transform.localScale = Vector3.one;
            float elapsedTime = 0f; //FIXME: make this a constant so it doesn't add more load on the runtime or use DoTween
            float duration = 1f; //FIXME: make this a constant so it doesn't add more load on the runtime or use DoTween
            Vector3 initialScale = new Vector3(1.25f, 1.25f, 1.25f); //FIXME: make this a constant so it doesn't add more load on the runtime
            Vector3 targetScale = Vector3.one;

            while (elapsedTime < duration)
            {
                rect.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rect.localScale = targetScale;
            if (playName == uid)
            {
                //FIXME: make this 'NewBux' a constant so it doesn't add more load on the runtime
                ShowDisplayName("NewBux" + NetworkClient.localPlayer.netId);
                OpenChangeNameDialog();
            }

            miniMap.SetActive(true);

            if (PlayerPrefs.GetString("isFirstTime") == "")
            {
                dialogTempleteController.SetData(tutorialDialogData);
                dialogTempleteController.OpenDialogTemplate();
                PlayerPrefs.SetString("isFirstTime", "No");
                StartCoroutine(OpenNews());

            }
            else
            {
                playbuxNews.Open();
            }
            /*try
            {
                FirebaseAuthenticationService.GetInstance();
            }
            catch
            {

            }*/

        }

        IEnumerator OpenNews()
        {
            yield return new WaitUntil(() => !dialogTempleteController.gameObject.activeSelf);
            playbuxNews.Open();
        }

        private async UniTask TickCounter()
        {
            while (true)
            {
                await UniTask.WaitForSeconds(1f);
                serverTicks += 10_000_000;

                if (serverTicks >= identitySystem[NetworkClient.localPlayer.netId].LoginTime.Ticks + (fifteenMin * 10_000_000))
                {

                    if (flagCollection.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, "2_END") == null &&
                        flagCollection.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, "2_START") == null) // if already have flag return;
                    {
                        NetworkClient.Send(new QuestMessage(NetworkClient.localPlayer.netId, "dailyquest," + "2_START"));
                    }

                }

            }
        }

        private IEnumerator TokenCheck()
        {
            yield return new WaitUntil(() => NetworkClient.localPlayer != null);
            yield return new WaitUntil(() => identitySystem.ContainsKey(NetworkClient.localPlayer.netId));
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId] != null);
            var buffer = identitySystem[NetworkClient.localPlayer.netId];
            yield return new WaitUntil(() => buffer.DisplayName != null);
#if LOGIN_BYPASS
            yield return new WaitUntil(() => !loginPage.gameObject.activeInHierarchy);
#endif
#if !LOGIN_BYPASS
            yield return new WaitUntil(() => !logInFromWeb.gameObject.activeInHierarchy);
#endif
            yield return new WaitUntil(() => accessTokenExpireTime.Ticks != 0);
            FireIdentitySignal(PlayerPrefs.GetString(TokenUtility.accessTokenKey));

            while (true)
            {

                if (accessTokenExpireTime.Ticks < DateTime.UtcNow.AddMinutes(3).Ticks)
                {
                    yield return RefreshTokenCheck().ToCoroutine();
                }

                if (refreshTokenExpireTime.Ticks < DateTime.UtcNow.AddHours(2).Ticks) //FIXME: make this a constant so it doesn't add more load on the runtime
                {
                    //TODO: FIX WHEN REFRESHTOKEN EXPIRE DO WHAT
                    //loginPage.gameObject.SetActive(true);
                }

                refreshTokenExpireTime = TokenUtility.GetExpiryDate(TokenUtility.expiryRefeshTokenDateKey);
                refreshToken = PlayerPrefs.GetString(TokenUtility.refreshTokenKey);
                accessTokenExpireTime = TokenUtility.GetExpiryDate(TokenUtility.expiryAccessTokenDateKey);
                accessToken = PlayerPrefs.GetString(TokenUtility.accessTokenKey);
                yield return new WaitForSeconds(60f); //FIXME: make this a constant so it doesn't add more load on the runtime
            }


        }

        private async UniTask RefreshTokenCheck()
        {
            var accessTokenResponse = await APIServerConnector.RefreshTokenAPI(refreshToken);
            string accessToken = TokenUtility.Decode(accessTokenResponse.accessToken);

            AccessTokenData accessTokenData = new AccessTokenData();
            accessTokenData = JsonConvert.DeserializeObject<AccessTokenData>(accessToken);

            DateTime dateTimeAccessToken = TokenUtility.SetDateTime(accessTokenData.exp);

            TokenUtility.SetAccessToken(accessTokenResponse.accessToken, dateTimeAccessToken);

            FireIdentitySignal(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
        }

        void FireIdentitySignal(string accessToken) //FIXME: always explicit 'private' keyword on a private method please
        {
            IdentityChangeSignal identityChangeSignal = new IdentityChangeSignal();
            identityChangeSignal.Command = "accesstoken"; //FIXME: make this a constant so it doesn't add more load on the runtime
            identityChangeSignal.Data = accessToken;
            signalBus.Fire(identityChangeSignal);
        }

        void ShowDisplayName(string inputName) //FIXME: always explicit 'private' keyword on a private method please
        {
            foreach (TextMeshProUGUI name in playerName)
            {
                name.text = inputName;
            }

        }


        IEnumerator QuestComplete() //FIXME: always explicit 'private' keyword on a private method please
        {
            questComplete.SetActive(true);
            Vector3 initialScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;
            float elapsedTime = 0f; //FIXME: make this a constant so it doesn't add more load on the runtime or use DoTween
            float duration = 1f; //FIXME: make this a constant so it doesn't add more load on the runtime or use DoTween
            while (elapsedTime < duration)
            {
                questComplete.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            questComplete.transform.localScale = targetScale;

            yield return new WaitForSeconds(3f); //FIXME: make this a constant so it doesn't add more load on the runtime

            Destroy(questComplete);
        }

        private void OnLinkOutSignalRecieve(LinkOutSignal signal)
        {
            var data = (DialogLinkOutData)signal.linkOutData;
            dialogTempleteController.SetData(data);
            dialogTempleteController.OpenDialogTemplate();
        }

        public static async void ApplicationClose()
        {
#if !UNITY_EDITOR
            await APIServerConnector.LogOutAPI(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteAll();
            Debug.Log("ApplicationClose()");
#endif
        }

        void OnApplicationQuit()
        {
            if (isQueue)
            {
                APIServerConnector.LeaveWaitingQueue(PlayerPrefs.GetString(TokenUtility.accessTokenKey)).Forget();
            }
            ApplicationClose();
        }

#endif
    }

}
