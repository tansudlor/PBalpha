
using System;
using UnityEngine;using com.playbux.ui.sortable;
using UnityEngine.EventSystems;
using Zenject;

namespace com.playbux.ui.draggable
{
    public class SimpleDragUI : MonoBehaviour, IDraggableUI
    {
        private bool isLock;
        private UICanvas uiCanvas;
        private ISortableUI sortableUI;
        private RectTransform dragTarget;

        [Inject]
        private void Construct(UICanvas uiCanvas, RectTransform dragTarget, ISortableUI sortableUI)
        {
            this.uiCanvas = uiCanvas;
            this.dragTarget = dragTarget;
            this.sortableUI = sortableUI;
        }

        public void SetLock(bool isLock)
        {
            this.isLock = isLock;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (isLock)
                return;

            var delta = eventData.delta / uiCanvas.Canvas.scaleFactor;
            Vector2 newPos = delta + dragTarget.anchoredPosition;
            dragTarget.anchoredPosition = newPos;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            sortableUI.BringToTop();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(Camera.main.ScreenToViewportPoint(dragTarget.anchoredPosition), Vector3.one);
        }
    }
}