using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
namespace com.playbux.sorting
{
    public class LineSortableFacade : MonoBehaviour
    {
        public ISortable Sortable => sortable;

        private ISortable sortable;

        [Inject]
        public void Construct(ISortable sortable)
        {
            this.sortable = sortable;
            Assert.IsNotNull(sortable);
        }
    }
}
