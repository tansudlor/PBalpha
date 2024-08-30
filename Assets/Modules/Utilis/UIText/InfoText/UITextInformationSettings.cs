using TMPro;
using System;
using UnityEngine;

namespace com.playbux.utilis.uitext
{
    [Serializable]
    public struct UITextInformationSettings
    {
        public int size;
        public bool isBold;
        public bool isItalic;
        public Color color;
        public TMP_FontAsset font;
    }
}