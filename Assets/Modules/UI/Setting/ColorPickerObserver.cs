using UnityEngine;

namespace com.playbux.ui.setting
{
    public abstract class ColorPickerObserver : MonoBehaviour
    {
        public abstract void SetColorFromPicker(Color color);
        
    }

}