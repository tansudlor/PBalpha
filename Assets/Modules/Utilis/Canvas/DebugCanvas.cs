using UnityEngine;

namespace com.playbux.utilis.canvas
{
    public class DebugCanvas : MonoBehaviour
    {
        public Transform BottomLeftCorner => bottomLeftCorner;

        [SerializeField]
        private Transform bottomLeftCorner;
    }
}