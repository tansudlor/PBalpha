using Zenject;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui.sortable
{
    public class SortableUI : ISortableUI, ILateDisposable
    {
        public int SortingOrder => canvas.sortingOrder;

        private readonly Canvas canvas;
        private readonly UILayerGroup uiLayerGroup;
        private readonly SortableController sortableController;
        private readonly Button[] bringToTopAreas;

        protected SortableUI(Canvas canvas, UILayerGroup uiLayerGroup, SortableController sortableController, Button[] bringToTopAreas)
        {
            this.canvas = canvas;
            this.uiLayerGroup = uiLayerGroup;
            this.sortableController = sortableController;
            this.bringToTopAreas = bringToTopAreas;

            for (int i = 0; i < this.bringToTopAreas.Length; i++)
            {
                this.bringToTopAreas[i].onClick.AddListener(BringToTop);
            }
        }

        public void BringToTop()
        {
            sortableController.Remove(uiLayerGroup, this);
            sortableController.Add(uiLayerGroup, this);
        }
        public void PushBack()
        {
            sortableController.Swap(uiLayerGroup, canvas.sortingOrder, this);
        }
        public void SetOrder(int sortingOrder)
        {
            canvas.sortingOrder = sortingOrder;
        }

        public void LateDispose()
        {
            for (int i = 0; i < bringToTopAreas.Length; i++)
            {
                bringToTopAreas[i].onClick.RemoveListener(BringToTop);
            }
        }
    }

    public abstract class MonoSortableUI : MonoBehaviour, ISortableUI
    {
        public virtual int SortingOrder => canvas.sortingOrder;

        [SerializeField]
        internal Canvas canvas;

        private UILayerGroup uiLayerGroup;
        private SortableController sortableController;

        [Inject]
        private void Construct(UILayerGroup uiLayerGroup, SortableController sortableController)
        {
            this.uiLayerGroup = uiLayerGroup;
            this.sortableController = sortableController;
        }

        public void BringToTop()
        {
            sortableController.Remove(uiLayerGroup, this);
            sortableController.Add(uiLayerGroup, this);
        }
        public void PushBack()
        {
            sortableController.Swap(uiLayerGroup, canvas.sortingOrder, this);
        }
        public void SetOrder(int sortingOrder)
        {
            canvas.sortingOrder = sortingOrder;
        }
    }
}