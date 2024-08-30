using System;
using UnityEngine;
namespace com.playbux.settings
{
    [Serializable]
    public class BubbleNumberSetting
    {
        [SerializeField]
        private int bubbleNum;

        public int BubbleNum { get => bubbleNum; set => bubbleNum = value; }
    }

}