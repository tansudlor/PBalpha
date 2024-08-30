
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

namespace com.playbux.minimap
{
    public class LocationBox : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI locationName;
        [SerializeField]
        private Image locationImage;

        private Vector3 locationPosition;

        private MiniMapLocator locator;

        private string locationDisplay;

        public TextMeshProUGUI LocationName { get => locationName; set => locationName = value; }
        public Image LocationImage { get => locationImage; set => locationImage = value; }
        public Vector3 LocationPosition { get => locationPosition; set => locationPosition = value; }
        public MiniMapLocator Locator { get => locator; set => locator = value; }
        public string LocationDisplay { get => locationDisplay; set => locationDisplay = value; }

        public void SetCenter()
        {

            if (LocationDisplay == "player")
            {
                locationPosition = NetworkClient.localPlayer.transform.position;
                Debug.Log(locationPosition);
            }
           
            Locator.FocusPoint = locationPosition;
        }

        

    }
}

