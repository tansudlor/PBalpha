using com.playbux.sfxwrapper;
using UnityEngine;

namespace com.playbux.networking.networkinventory
{
    public sealed class CloseButton : MonoBehaviour
    {
        public Transform Info;

        [SerializeField]
        private InventoryUIController controller; 

        public void CloseDialog()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            controller.Toggle();
            Info.gameObject.SetActive(false);
        }
        public void CloseInfo()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            Info.gameObject.SetActive(false);
        }
    }
}
