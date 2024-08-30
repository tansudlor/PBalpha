using TMPro;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.bubble
{
    public class InteractBubble : IBubble
    {
        private readonly RectTransform rect;
        private readonly TextMeshProUGUI text;

        private Camera mainCamera;
        private RectTransform canvasRectTransform;

        public InteractBubble(BubbleContainer container, RectTransform rect, TextMeshProUGUI text)
        {
            this.rect = rect;
            this.text = text;
            mainCamera = Camera.main;
            canvasRectTransform = container.RectTransform;
        }

        public void Initialize(string message, Vector3 position, Transform parent)
        {
            rect.SetParent(parent, true);
            UpdatePosition(position);
            rect.gameObject.SetActive(true);
            text.SetText(message);
        }

        public void Dispose()
        {
            rect.gameObject.SetActive(false);
        }

        public void UpdateText(string message)
        {
            text.SetText(message);
        }

        public void UpdatePosition(Vector3 position)
        {
            var screenPosition = mainCamera.WorldToScreenPoint(position);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out var canvasPosition))
                rect.localPosition = canvasPosition + -Vector2.up * 12;

            if (!rect.gameObject.activeInHierarchy)
                rect.gameObject.SetActive(true);
        }

        public void Show()
        {
            rect.gameObject.SetActive(true);
        }

        public void Hide()
        {
            rect.gameObject.SetActive(false);
        }

        public class Pool : MemoryPool<string, Vector3, Transform, IBubble>
        {
            protected override void Reinitialize(string message, Vector3 position, Transform parent, IBubble item)
            {
                item.Initialize(message, position, parent);
            }

            protected override void OnDespawned(IBubble item)
            {
                item.Dispose();
            }
        }
    }
}