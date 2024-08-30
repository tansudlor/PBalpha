using com.playbux.api;
using com.playbux.events;
using com.playbux.networking.mirror.infastructure;
using com.playbux.settings;
using com.playbux.sfxwrapper;
using Cysharp.Threading.Tasks;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace com.playbux.ui.gamemenu
{

    public enum Mode
    {
        LogOut,
        Quit
    }

    public class LogoutUICOntroller : MonoBehaviour
#if SERVER
    {
#endif
#if !SERVER

        , IPopUpUICOntroller
    {
        public GameObject LogOutPopUp;
        public Mode mode;

        private SignalBus signalBus;
#if LOGIN_BYPASS
        private LogInPage logInPage;
#endif
#if !LOGIN_BYPASS
        private LogInFromWeb logInFromWeb;
#endif
        private NetworkController networkController;
        private ISettingUIController settingUIController;
        private IGameMenuUiController gameMenuUiController;
        private SettingDataBase settingDataBase;
        [Inject]
        void SetUp(
#if LOGIN_BYPASS
            LogInPage logInPage,
#endif
#if !LOGIN_BYPASS
         LogInFromWeb logInFromWeb,
#endif
        SignalBus signalBus, ISettingUIController settingUIController, IGameMenuUiController gameMenuUiController, NetworkController networkController, SettingDataBase settingDataBase)
        {
#if LOGIN_BYPASS
            this.logInPage = logInPage;
#endif
#if !LOGIN_BYPASS
            this.logInFromWeb = logInFromWeb;
#endif
            this.signalBus = signalBus;
            this.settingUIController = settingUIController;
            this.gameMenuUiController = gameMenuUiController;
            this.networkController = networkController;
            this.settingDataBase = settingDataBase;
        }


        public void ButtonTrigger()
        {

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (mode == Mode.LogOut)
            {
                LogOut();
            }

            else if (mode == Mode.Quit)
            {
                QuitGame();
            }

        }

        private async void QuitGame()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            await APIServerConnector.LogOutAPI(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
            PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
            Application.Quit();
        }

        private async void LogOut()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            await APIServerConnector.LogOutAPI(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
            PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
            Application.Quit();
            return;
            signalBus.Fire(new LogoffSignal());
            networkController.Manager.StopClient();
#if LOGIN_BYPASS
            logInPage.ShowLogInPage();
#endif
#if !LOGIN_BYPASS
           logInFromWeb.ShowLogInPage();
#endif
            ClosePopUp();
            settingUIController.CloseSetting();
            gameMenuUiController.PlayerChangeID();
            gameMenuUiController.ShowGameMenu();

        }

        public void ClosePopUp()
        {
            LogOutPopUp.SetActive(false);
        }
#endif
    }
}
