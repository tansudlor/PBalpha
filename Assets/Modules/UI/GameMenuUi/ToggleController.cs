using com.playbux.settings;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui.gamemenu
{
    public class ToggleController : MonoBehaviour
    {
        public bool isOn;

        public Image toggleBgImage;
        public RectTransform toggle;

        public Image HandleImage;
        public Sprite GreenHandleImage;
        public Sprite DefaultHandleImage;
        public RectTransform handleTransform;

        private float handleSize;
        private float onPosX;
        private float offPosX;

        public float handleOffset;

        public GameObject onIcon;
        public GameObject offIcon;


        public float speed;
        static float t = 0.0f;

        private bool switching = false;

        private bool isFullscreen = true;
        private ISettingUIController settingUIController;


        [Inject]
        void SetUp(ISettingUIController settingUIController)
        {
            this.settingUIController = settingUIController;

        }

        void Start()
        {
            RectTransform handleRect = handleTransform;
            handleSize = handleRect.sizeDelta.x;
            float toggleSizeX = toggle.sizeDelta.x;
            onPosX = (toggleSizeX * 0.5f) - (handleSize * 0.5f);
            offPosX = onPosX * -1;



            uint appMode = settingUIController.SettingDataBase.ScreenSetting;

            Debug.Log(appMode + " appMode");
            isOn = (appMode == (uint)PCAppMode.Windowed);

            onIcon.SetActive(true);
            offIcon.SetActive(true);

            if (isOn)
            {

                handleTransform.localPosition = new Vector3(onPosX, -0.66191f, 0f);
                onIcon.gameObject.SetActive(true);
                offIcon.gameObject.SetActive(false);
            }
            else
            {

                handleTransform.localPosition = new Vector3(offPosX, -0.66191f, 0f);
                onIcon.gameObject.SetActive(false);
                offIcon.gameObject.SetActive(true);
            }

            SetFullScreenMode(!isOn);

        }

        void Update()
        {

            if (switching)
            {
                Toggle(isOn);
            }
        }

        public void SetFullScreenMode(bool fullScreen)
        {

            if (fullScreen)
            {
                Debug.Log(fullScreen + " fullScreen");
                settingUIController.SettingDataBase.SetBoardcast(SettingVariable.ScreenSetting, PCAppMode.Fullscreen);
                SettingDataBase.SaveSetting(settingUIController.SettingDataBase);

                for (int i = 0; i < Screen.resolutions.Length; i++)
                {

                    var resolution = Screen.resolutions[i];
                    var ratio = resolution.width * 1f / resolution.height * 1f;
                    var currentRatio = Screen.currentResolution.width * 1f / Screen.currentResolution.height * 1f;
                    Debug.Log(ratio + " : " + currentRatio);

                    if (ratio == currentRatio)
                    {
                        Debug.Log(resolution.width + " ::: " + resolution.height);
                        if (resolution.height == 1080)
                        {
                            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
                            break;
                        }

                        if (resolution.width == 1920)
                        {

                            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
                            break;

                        }

                    }



                }
            }
            else
            {
                Debug.Log(fullScreen + " fullScreen");
                settingUIController.SettingDataBase.SetBoardcast(SettingVariable.ScreenSetting, PCAppMode.Windowed);
                SettingDataBase.SaveSetting(settingUIController.SettingDataBase);
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            }


        }

        public void Switching()
        {
            switching = true;
        }



        public void Toggle(bool toggleStatus)
        {
            onIcon.SetActive(true);
            offIcon.SetActive(true);

            if (toggleStatus)
            {

                Transparency(onIcon, 1f, 0f);
                Transparency(offIcon, 0f, 1f);
                HandleImage.sprite = DefaultHandleImage;
                handleTransform.localPosition = SmoothMove(onPosX, offPosX);
            }
            else
            {
                Transparency(onIcon, 0f, 1f);
                Transparency(offIcon, 1f, 0f);
                HandleImage.sprite = GreenHandleImage;

                handleTransform.localPosition = SmoothMove(offPosX, onPosX);
            }

        }


        Vector3 SmoothMove(float startPosX, float endPosX)
        {

            Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
            StopSwitching();
            return position;
        }

        CanvasGroup Transparency(GameObject alphaObj, float startAlpha, float endAlpha)
        {
            CanvasGroup alphaVal;
            alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
            alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
            alphaVal.alpha = endAlpha;
            return alphaVal;
        }

        void StopSwitching()
        {
            if (t > 1.0f)
            {
                switching = false;

                t = 0.0f;
                switch (isOn)
                {
                    case true:
                        SetFullScreenMode(isOn);
                        isOn = false;
                        break;

                    case false:
                        SetFullScreenMode(isOn);
                        isOn = true;
                        break;
                }

            }
        }

    }
}