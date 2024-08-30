using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using com.playbux.settings;
using com.playbux.io;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using com.playbux.schedulessetting;

using AYellowpaper.SerializedCollections;
using UnityEngine.Audio;
using com.playbux.sfxwrapper;
using com.playbux.analytic;
using com.playbux.api;
using TMPro;

namespace com.playbux.ui.gamemenu
{

    public class SettingUIController : MonoBehaviour, ISettingUIController, ISettingDataBaseReader
    {
        public GameObject SettingPopUp;
        public GameObject QuitPopUp;

        public Image SpeakerImage;
        public Sprite SpeakerNormal;
        public Sprite SpeakerMute;

        public Slider SpeakerSlider;
        public Image SliderHandle;
        public Sprite GrayHandle;
        public Sprite GreenHandle;

        public Image SliderFill;

        public Color MuteColor;
        public Color UnMuteColor;

        public TextMeshProUGUI versionText;

        private const string MASTER_VOLUME_SAVE_KEY = "master";
        private const string MASTER_VOLUME_MIXER_KEY = "MasterVolume";
        private const int MINIMUM_VOLUME_VALUE = -60;
        private const int MAXIMUM_VOLUME_VALUE = 0;

        [SerializeField]
        private AudioMixer mixer;

        private bool isMute = false;
        private SettingDataBase settingDataBase;

        public SettingDataBase SettingDataBase { get => settingDataBase; set => settingDataBase = value; }

        [Inject]
        private void SetUp(SettingDataBase settingDataBase)
        {
            SettingDataBase = settingDataBase;
        }

        // Start is called before the first frame update
        void Start()
        {
            SettingDataBase.NotAllowWriteForThisType = true;
            StartAsync();
            versionText.text = "version : " + Application.version;
        }

        void StartAsync()
        {
            try
            {

                SettingDataBase.LoadSettingTo(this);
                Debug.Log(JsonConvert.SerializeObject(SettingDataBase));
            }
            catch
            {

                SettingDataBase.SaveSetting(SettingDataBase);
            }

            CloseSetting();

            SettingDataStatic.settingDataBase = SettingDataBase;
            SettingDataBase.NotAllowWriteForThisType = false;
            var sliderValue = SettingDataBase.AudioSettings;
            SpeakerSlider.value = sliderValue.AudioLevels[MASTER_VOLUME_SAVE_KEY];
            
            isMute = sliderValue.Mute;
            MuteVoice();
        }



        public void OpenSetting()
        {

            if (!SettingPopUp.activeInHierarchy)
            {
                SettingPopUp.SetActive(true);
            }
            else
            {
                SettingPopUp.SetActive(false);
            }
        }

        public void CloseSetting()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            SettingPopUp.SetActive(false);
        }

        public void OpenQuit()
        {
            try
            {
                AnalyticWrapper.getInstance().Log("click_button",
                    new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                     , new LogParameter("user_amount", "1")
                     , new LogParameter("button_type", "quit")
                     );
            }
            catch
            {

            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            QuitPopUp.SetActive(true);
            // SettingPopUp.transform.localScale = Vector3.zero;
        }

        public void MuteVoice()
        {


            if (isMute)
            {
                SliderHandle.sprite = GrayHandle;
                SpeakerImage.sprite = SpeakerMute;
                SliderFill.color = MuteColor;

                OnAudioSetting(true, SpeakerSlider.value,-80f);
                isMute = false;

            }
            else
            {
                SliderHandle.sprite = GreenHandle;
                SpeakerImage.sprite = SpeakerNormal;
                SliderFill.color = UnMuteColor;
                float calculatedValue = VolumeValue();
                OnAudioSetting(false, SpeakerSlider.value, calculatedValue);
                isMute = true;

            }
        }

        public void OnMasterVolumeValueChange()
        {
            SliderHandle.sprite = GreenHandle;
            SpeakerImage.sprite = SpeakerNormal;
            SliderFill.color = UnMuteColor;
            float calculatedValue = VolumeValue();
            OnAudioSetting(false, SpeakerSlider.value, calculatedValue);
        }

        private void  OnAudioSetting(bool isMute,float audioLevel,float audioSound)
        {
            settings.AudioSettings audioSettings = new settings.AudioSettings();
            audioSettings.Mute = isMute;
            audioSettings.AudioLevels[MASTER_VOLUME_SAVE_KEY] = audioLevel;
            settingDataBase.AudioSettings = audioSettings;
            mixer.SetFloat(MASTER_VOLUME_MIXER_KEY, audioSound);
            PlayerPrefs.SetFloat("MasterVolumn", audioLevel);
            SettingDataBase.SaveSetting(settingDataBase);
            settingDataBase.SetBoardcast(SettingVariable.AudioSettings, audioSettings);
        }

        private float VolumeValue()
        {
            float calculatedValue = MINIMUM_VOLUME_VALUE + (SpeakerSlider.value - MAXIMUM_VOLUME_VALUE) * Mathf.Abs(MINIMUM_VOLUME_VALUE);

            if (calculatedValue <= MINIMUM_VOLUME_VALUE)
                calculatedValue = -80;
            return calculatedValue;
        }
    }
}