namespace com.playbux.sorting
{
    public interface ISortComponent
    {
        int SortingOrder { get; }
        void Sort(int order);
    }
}