using UnityEngine.Rendering;

namespace com.playbux.sorting
{
    public class GroupSortComponent : ISortComponent
    {
        public int SortingOrder => sortingGroup.sortingOrder;

        private readonly SortingGroup sortingGroup;

        public GroupSortComponent(SortingGroup sortingGroup)
        {
            this.sortingGroup = sortingGroup;
        }

        public void Sort(int order) => sortingGroup.sortingOrder = order;
    }
}