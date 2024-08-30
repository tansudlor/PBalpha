using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.ui
{
    [CreateAssetMenu(fileName = "TextDataForWaitQueue", menuName = "UIDialogData/TextDataForWaitQueue")]
    public class TextDataForWaitQueue : ScriptableObject
    {
        
        [SerializeField]
        private string sponserData;
        [SerializeField]
        private string url;

        public string SponserData { get => sponserData; set => sponserData = value; }
        public string Url { get => url; set => url = value; }
    }
}
