using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using com.playbux.settings;
using System;
using UnityEditor;
using com.playbux.events;
using Newtonsoft.Json;
using com.playbux.ui.gamemenu;
namespace com.playbux.ui.setting
{

    public class ColorPickerInteract : ColorPickerObserver
    {
        [SerializeField]
        private Sprite eyeClose;
        [SerializeField]
        private Sprite eyeOpen;
        [SerializeField]
        private Image eyeIcon;

        [SerializeField]
        private Image colorImage;

       
        private ISettingUIController settingUIController;
        private SignalBus signalBus;
        
        [Inject]
        void SetUp(ISettingUIController settingUIController,SignalBus signalBus)
        {
            this.settingUIController = settingUIController;
            this.signalBus = signalBus;
           
        }

        private void Start()
        {
            DisplayNameSetting nameSetting = new DisplayNameSetting();
            var targetSettings = this.gameObject.name.Split('.')[0]; // extract property from gameObject name, which Control the Property
            nameSetting = GetNameSetting(targetSettings);
            colorImage.color = nameSetting.Color;

            if (nameSetting.IsShow == true)
            {
                eyeIcon.sprite = eyeOpen;
            }
            else
            {
                eyeIcon.sprite = eyeClose;
            }


           

        }

        public override void SetColorFromPicker(Color color)
        {
            var targetSettings = this.gameObject.name.Split('.')[0]; // extract property from gameObject name, which Control the Property
            colorImage.color = color;
            SetColorName(targetSettings, color);
        }

        public void SetEyeIcon(Image eyeIcon)
        {
            var targetSettings = this.gameObject.name.Split('.')[0];
            if (eyeIcon.sprite == eyeOpen)
            {
                eyeIcon.sprite = eyeClose;
                SetNameVisibility(targetSettings, false);
            }
            else
            {
                eyeIcon.sprite = eyeOpen;
                SetNameVisibility(targetSettings, true);
            }

        }


        private DisplayNameSetting GetNameSetting<T>(T settingVariable)
        {
            //SettingDataBase settingDataBase =  JsonConvert.DeserializeObject<SettingDataBase>(PlayerPrefs.GetString("settingdata"));
            DisplayNameSetting nameSetting = (DisplayNameSetting)settingUIController.SettingDataBase.GetData(settingVariable);
            return nameSetting;
        }

        private void SetColorName<T>(T settingVariable, Color color)
        {
            DisplayNameSetting nameSetting = GetNameSetting(settingVariable);
            DisplayNameSetting newNameSetting = new DisplayNameSetting();
            newNameSetting.Color = color;
            newNameSetting.IsShow = nameSetting.IsShow;
            settingUIController.SettingDataBase.SetBoardcast(settingVariable, newNameSetting);
            SettingDataBase.SaveSetting(settingUIController.SettingDataBase);
            SettingDataBoardcast(settingVariable.ToString(), newNameSetting);
        }

        private void SetNameVisibility<T>(T settingVariable, bool isShow)
        {
            Debug.Log(isShow + " isshow");
            DisplayNameSetting nameSetting = GetNameSetting(settingVariable);
            DisplayNameSetting newNameSetting = new DisplayNameSetting();
            newNameSetting.Color = nameSetting.Color;
            newNameSetting.IsShow = isShow;
            settingUIController.SettingDataBase.SetBoardcast(settingVariable, newNameSetting);
            SettingDataBase.SaveSetting(settingUIController.SettingDataBase);
            SettingDataBoardcast(settingVariable.ToString(), newNameSetting);
        }

        private void SettingDataBoardcast(string commad , object data)
        {
            SettingDataSignal settingDataSignal = new SettingDataSignal();
            settingDataSignal.Command = commad;
            settingDataSignal.Data = data;
            signalBus.Fire(settingDataSignal);
        }
    }

}
