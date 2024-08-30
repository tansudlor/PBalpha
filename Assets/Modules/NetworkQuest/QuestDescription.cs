using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.networkquest
{
    public class QuestDescription : MonoBehaviour
    {
        [SerializeField]
        private Image checkBox;

        [SerializeField]
        private Sprite greenCheckBox;

        public void ChangeCheckBox()
        {
            checkBox.sprite = greenCheckBox;
        }
    }
}