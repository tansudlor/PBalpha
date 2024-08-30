using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.ui
{

    [CreateAssetMenu(fileName = "DialogData", menuName = "UIDialogData/DialogData")]
    public class DialogLinkOutData : ScriptableObject, IDialogData
    {
        [SerializeField]
        private Sprite header;
        [SerializeField]
        private Sprite[] imageContents;
        [SerializeField]
        private string[] stringContents;
        [SerializeField]
        private Sprite imageConfirm;
        [SerializeField]
        private string readMoreUrl;
        [SerializeField]
        private string confirmUrl;
       

        public Sprite Header { get => header; set => header = value; }
        public Sprite[] ImageContents { get => imageContents; set => imageContents = value; }
        public string[] StringContents { get => stringContents; set => stringContents = value; }
        public Sprite ImageConfirm { get => imageConfirm; set => imageConfirm = value; }
        public string ReadMoreUrl { get => readMoreUrl; set => readMoreUrl = value; }
        public string ConfirmUrl { get => confirmUrl; set => confirmUrl = value; }
      
    }
}
