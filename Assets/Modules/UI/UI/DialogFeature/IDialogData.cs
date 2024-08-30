using UnityEngine;

namespace com.playbux.ui
{
    public interface IDialogData
    {
        Sprite Header { get; set; }
        Sprite[] ImageContents { get; set; }
        string[] StringContents { get; set; }
        Sprite ImageConfirm { get; set; }
        string ReadMoreUrl { get; set; }
        string ConfirmUrl { get; set; }

    }
}
