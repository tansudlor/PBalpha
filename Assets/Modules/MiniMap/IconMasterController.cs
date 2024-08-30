
using Zenject;
using UnityEngine;
using com.playbux.events;
using UnityEngine.UI;

namespace com.playbux.minimap
{
    public class IconMasterController : MonoBehaviour
    {
        public string IconName { get => iconName; set => iconName = value; }
        public MiniMapLocator MiniMapLocator { get => miniMapLocator; set => miniMapLocator = value; }
        public SignalBus SignalBus { get => signalBus; set => signalBus = value; }

        [SerializeField]
        private string iconName;

        [SerializeField]
        private Button iconButton;

        [SerializeField]
        private MiniMapLocator miniMapLocator;

        private SignalBus signalBus;

        public void ShowName()
        {

            if(IconName != "pinpoint")
            {
               
            }
            else
            {
                miniMapLocator.Data.GetIconByName(IconName)[0].Group = IconGroup.NONE;
                signalBus.Fire(new RefreshIconSignal());
            }
        }
    }
}
