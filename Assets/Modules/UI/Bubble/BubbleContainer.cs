using System;
using UnityEngine;

namespace com.playbux.ui.bubble
{
    public class BubbleContainer
    {
        public Transform Transform => canvas.transform;
        public RectTransform RectTransform => rectTransform;
        public Transform[] Channels => subContainers;
        
        private readonly Canvas canvas;
        private readonly CanvasGroup canvasGroup;
        private readonly Transform[] subContainers;
        private readonly RectTransform rectTransform;
        
        public BubbleContainer(UICanvas uiCanvas, Canvas canvas, CanvasGroup canvasGroup, RectTransform rectTransform, Transform[] subContainers)
        {
            this.canvas = canvas;
            this.canvasGroup = canvasGroup;
            this.subContainers = subContainers;
            this.rectTransform = rectTransform;
            Transform.SetParent(uiCanvas.transform);
        }

        public void SetInteractable(bool interactable)
        {
            canvasGroup.interactable = interactable;
        }
    }
}
