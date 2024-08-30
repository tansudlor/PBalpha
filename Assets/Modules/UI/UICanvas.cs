using UnityEngine;

namespace com.playbux.ui
{
    public class UICanvas : MonoBehaviour
    {
        public Canvas Canvas => canvas;

        public RectTransform RectTransform => rectTransform;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private RectTransform rectTransform;
    }
}