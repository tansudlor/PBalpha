using UnityEngine;
using com.playbux.sorting;

namespace com.playbux.networking.mirror.client.building
{
    public class PrefabBuilding : MonoBehaviour, IBuilding
    {
        public Vector3 Position => transform.position;
        public ISortable Sortable => facade.Sortable;

        [SerializeField]
        private LineSortableFacade facade;

        public virtual void Initialize()
        {
            transform.gameObject.SetActive(false);
        }

        public void Hide()
        {
            transform.gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            facade.Sortable.Initialize();
            transform.gameObject.SetActive(true);
        }

        public void Dispose()
        {
            Destroy(transform.gameObject);
        }
    }
}