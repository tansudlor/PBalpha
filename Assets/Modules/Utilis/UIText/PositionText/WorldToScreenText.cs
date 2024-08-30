using TMPro;
using Zenject;
using UnityEngine;

namespace com.playbux.utilis.uitext
{
    public class WorldToScreenText : MonoBehaviour
    {
        [SerializeField]
        private RectTransform rect;

        [SerializeField]
        private TextMeshProUGUI text;

        private Camera mainCamera;
        private RectTransform canvasRectTransform;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Initialize(Vector3 position, Transform parent)
        {
            canvasRectTransform = parent.GetComponent<RectTransform>();
            rect.SetParent(parent);
            rect.position = position;
            rect.gameObject.SetActive(true);
        }

        public void UpdatePosition(Vector3 position)
        {
            var screenPosition = mainCamera.WorldToScreenPoint(position);
            text.SetText($"x: {position.x:F3}, y: 0, z: {position.z:F3}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out var canvasPosition))
                rect.localPosition = canvasPosition + -Vector2.up * 12;

            if (!rect.gameObject.activeInHierarchy)
                rect.gameObject.SetActive(true);
        }

        private void Dispose()
        {
            rect.gameObject.SetActive(false);
        }

        public class Pool : MemoryPool<Vector3, Transform, WorldToScreenText>
        {
            protected override void Reinitialize(Vector3 position, Transform parent, WorldToScreenText text)
            {
                text.Initialize(position, parent);
            }

            protected override void OnDespawned(WorldToScreenText item)
            {
                item.Dispose();
            }
        }
    }
}