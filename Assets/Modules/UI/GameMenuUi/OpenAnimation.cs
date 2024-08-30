using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using com.playbux.events;
using com.playbux.api;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using com.playbux.sfxwrapper;
using System;

namespace com.playbux.ui.gamemenu
{
    public class OpenAnimation : MonoBehaviour
    {

        [SerializeField]
        private SkeletonGraphic backGoundSkeleton;
        [SerializeField]
        private SkeletonGraphic playbuxLogoSkeleton;
        [SerializeField]
        private SkeletonGraphic playbuxLogoLogInSkeleton;
        [SerializeField]
        private SkeletonGraphic clickToLogInSkeleton;
        [SerializeField]
        private VideoPlayer video;
        [SerializeField]
        private RawImage rawImageVideo;
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private GameObject buttonSkip;

        public string testStatus = "";

        private bool vdoFadeOut = false;
        private bool isFinish = false;

        public float loadingTime = 5f;
        public float fadeSpeed = 0.1f;

        public static bool isClick = false;
        public bool isFinishOpenLogo = false;
        private SignalBus signalBus;
        private LogInFromWeb logInFromWeb;

        
        private bool openingAnimationFinish = false;
        public SkeletonGraphic PlaybuxLogoLogInSkeleton { get => playbuxLogoLogInSkeleton; set => playbuxLogoLogInSkeleton = value; }
        public SkeletonGraphic ClickToLogInSkeleton { get => clickToLogInSkeleton; set => clickToLogInSkeleton = value; }
        public bool IsFinish { get => isFinish; set => isFinish = value; }

#if !LOGIN_BYPASS
        [Inject]
        void SetUp(LogInFromWeb logInFromWeb, SignalBus signalBus)
        {
            this.logInFromWeb = logInFromWeb;
            this.signalBus = signalBus;
           
        }

        public async void Start()
        {
#if !UNITY_EDITOR
            await APIServerConnector.LogOutAPI(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.accessTokenKey);
            PlayerPrefs.DeleteKey(TokenUtility.refreshTokenKey);
#endif
            var currentTransform = transform.parent;
            transform.SetParent(Camera.main.transform);
            transform.SetParent(currentTransform);
            playbuxLogoSkeleton.AnimationState.SetAnimation(0, "Logo_StartLoading3", false);
            playbuxLogoSkeleton.AnimationState.Complete += OnPlaybuxLogoComplete;
            video.loopPointReached += OnVideoEnd;
            PlaybuxLogoLogInSkeleton.AnimationState.Complete += OnPlaubuxLoginComplete;
            ClickToLogInSkeleton.AnimationState.Complete += OnPlayGame;
            audioSource.PlayDelayed(0.05f);

            float volumn = 0.0f;

            Debug.Log("audioSource.volume " + audioSource.volume);
            Debug.Log("video.GetDirectAudioVolume(0) " + video.GetDirectAudioVolume(0));

            if (PlayerPrefs.HasKey("MasterVolumn"))
            {
                volumn = PlayerPrefs.GetFloat("MasterVolumn");
            }
            else
            {
                volumn = 1.0f;
            }
            Debug.Log("volumn " + volumn);
            audioSource.volume = volumn;
            video.SetDirectAudioVolume(0, volumn / 2f);

            Debug.Log("audioSource.volume " + audioSource.volume);
            Debug.Log("video.GetDirectAudioVolume(0) " + video.GetDirectAudioVolume(0));
        }

        public void CloseVideo()
        {

            if (!isFinishOpenLogo)
            {
                return;
            }

            video.time = 22.00;

            isFinishOpenLogo = false;
            buttonSkip.SetActive(false);
        }




        public void CloseThisAnim()
        {
            Destroy(this.gameObject, 0.1f);

        }


        public void OnPlaybuxLogoComplete(TrackEntry trackEntry)
        {
            Debug.Log("trackEntry");
            if (trackEntry.Animation.Name == "Logo_StartLoading3")
            {
                if (openingAnimationFinish)
                {
                    return;
                }
                Debug.Log("trackEntryLoading");
                openingAnimationFinish = true;
                playbuxLogoSkeleton.AnimationState.SetAnimation(0, "Loading", true);
                LoadingTime().Forget();
                playbuxLogoSkeleton.AnimationState.Complete -= OnPlaybuxLogoComplete;
            }
        }

        async UniTask LoadingTime()
        {

            await UniTask.WaitForSeconds(1f);
            playbuxLogoSkeleton.AnimationState.SetAnimation(0, "Loading_FadeOut", false);
            backGoundSkeleton.AnimationState.SetAnimation(0, "FadeOut", false);
            video.gameObject.SetActive(true);
            isFinishOpenLogo = true;
            await UniTask.WaitUntil(() => video.time >= 22.12);
            PlaybuxLogoLogInSkeleton.gameObject.SetActive(true);
            ClickToLogInSkeleton.gameObject.SetActive(true);
            PlaybuxLogoLogInSkeleton.AnimationState.SetAnimation(0, "Start", false);
            ClickToLogInSkeleton.AnimationState.SetAnimation(0, "Start", false);
            SFXWrapper.getInstance().DisableMode = false;

            StartCoroutine(ShowStartLoginButton());

            /*if (string.IsNullOrEmpty(PlayerPrefs.GetString(TokenUtility.accessTokenKey)))//no token
            {
               StartCoroutine(StartLoginButton());
            }
            else
            {
                JObject queueData = await APIServerConnector.QueueMe(PlayerPrefs.GetString(TokenUtility.accessTokenKey));

                if (queueData == null)
                {
                    StartCoroutine(StartLoginButton());
                }
                else
                {

                    string status = "";
                    try
                    {
                        status = queueData["data"]["status"].ToString();
                    }
                    catch (Exception e)
                    {
                        status = e.Message;
                    }


                    if (status == "waiting")
                    {

                        logInFromWeb.AuthenticateToGameServer(PlayerPrefs.GetString(TokenUtility.accessTokenKey)).Forget();
                        return;
                    }
                    else if (status == "connecting")
                    {
                        PlaybuxLogoLogInSkeleton.gameObject.SetActive(false);
                        ClickToLogInSkeleton.gameObject.SetActive(false);
                        logInFromWeb.GamePage(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
                        return;
                    }
                    else if (status == "playing")
                    {
                        PlaybuxLogoLogInSkeleton.gameObject.SetActive(true);
                        ClickToLogInSkeleton.gameObject.SetActive(false);
                        StartCoroutine(ShowNotiDialog("You've already played this game", PlayerPrefs.GetString(TokenUtility.accessTokenKey)));
                        return;
                    }
                    else
                    {
                        PlaybuxLogoLogInSkeleton.gameObject.SetActive(false);
                        ClickToLogInSkeleton.gameObject.SetActive(false);
                        StartCoroutine(ShowNotiDialog(status, PlayerPrefs.GetString(TokenUtility.accessTokenKey)));
                        return;
                    }
                }
               

            }*/

            await UniTask.WaitUntil(() => video.time >= 22.45);
            buttonSkip.SetActive(false);

            rawImageVideo.color = new Color(1f, 1f, 1f, 0.5f);

            while (true)
            {
                try
                {
                    rawImageVideo.color = new Color(1f, 1f, 1f, rawImageVideo.color.a - fadeSpeed);
                }
                catch
                {

                }
                if (rawImageVideo.color.a <= 0)
                {

                    break;
                }

                await UniTask.WaitForSeconds(0.01f);
            }
            try
            {
                rawImageVideo.color = new Color(1f, 1f, 1f, 0f);
            }
            catch
            {

            }
            await UniTask.WaitForSeconds(1f);

            signalBus.Fire(new BGMPlaySignal("BGM/Login", SETHD.Echo.PlayMode.StartOver));

        }



        void OnVideoEnd(VideoPlayer vp)
        {

            video.loopPointReached -= OnVideoEnd;
            video.gameObject.SetActive(false);

        }

        public void OnPlaubuxLoginComplete(TrackEntry trackEntry)
        {
            Debug.Log("trackEntryIdle_00");
            if (trackEntry.Animation.Name == "Start")
            {
                PlaybuxLogoLogInSkeleton.AnimationState.SetAnimation(0, "Idle_00", true);
                PlaybuxLogoLogInSkeleton.AnimationState.Complete -= OnPlaubuxLoginComplete;
            }
        }

        public void OnPlayGame(TrackEntry trackEntry)
        {
            Debug.Log("trackEntry 555Idle_00");

            if (trackEntry.Animation.Name == "Start")
            {

                PlaybuxLogoLogInSkeleton.AnimationState.SetAnimation(0, "Idle_00", true);
                PlaybuxLogoLogInSkeleton.AnimationState.Complete -= OnPlayGame;


            }
        }


        IEnumerator ShowStartLoginButton()
        {
            yield return new WaitForSeconds(1.33f);
            logInFromWeb.ShowLogInPage();
            isFinish = true;
        }

        IEnumerator ShowNotiDialog(string desc, string accessToken)
        {
            yield return new WaitForSeconds(1.4f);
            logInFromWeb.ShowNotiPage(desc, accessToken, logInFromWeb.QuitGame);

        }


#endif
    }

}
