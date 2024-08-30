namespace com.playbux.ui.sortable
{
    public interface ISortableUI
    {
        int SortingOrder { get; }
        void BringToTop();
        void PushBack();
        void SetOrder(int sortingOrder);
    }
}