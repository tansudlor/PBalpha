using com.playbux.settings;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui.gamemenu
{
    public class ChatBubbleController : MonoBehaviour
    {

        [SerializeField]
        private Slider bubbleSlider;
        [SerializeField]
        private TMP_Text bubbleText;

        private int bubleValue;
        private SettingDataBase settingDataBase;

        [Inject]
        void SetUp(SettingDataBase settingDataBase)
        {
            this.settingDataBase = settingDataBase;
        }


        void Start()
        {
            var bubbleNumberSetting = settingDataBase.BubbleNumberSetting;
            bubbleSlider.value = bubbleNumberSetting.BubbleNum;
            bubbleText.text = bubbleSlider.value.ToString();
        }

        // Update is called once per frame
        public void OnBubbleValueChange()
        {
            bubbleText.text = bubbleSlider.value.ToString();
            BubbleNumberSetting bubbleNumberSetting = settingDataBase.BubbleNumberSetting;
            bubbleNumberSetting.BubbleNum = Mathf.RoundToInt(bubbleSlider.value);
            settingDataBase.SetBoardcast(SettingVariable.BubbleNumberSetting, bubbleNumberSetting);
        }
    }
}
