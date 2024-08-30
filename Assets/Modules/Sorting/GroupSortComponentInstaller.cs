using UnityEngine;
using UnityEngine.Rendering;

namespace com.playbux.sorting
{
    public class GroupSortComponentInstaller : BaseSortComponentInstaller<GroupSortComponent>
    {
        [SerializeField]
        private SortingGroup sortingGroup;

        internal override void BindComponent()
        {
            Container.Bind<SortingGroup>().FromInstance(sortingGroup).AsSingle();
        }
    }
}