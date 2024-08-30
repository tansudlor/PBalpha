using Zenject;
using UnityEngine;
using com.playbux.sorting;

namespace com.playbux.networking.mirror.client.prop
{
    public class ClientProp : MonoBehaviour
    {
        public ISortable Sortable => sortable;

        private ISortable sortable;

        [Inject]
        public void Construct(ISortable sortable)
        {
            this.sortable = sortable;
        }

        public class Factory : PlaceholderFactory<Object, Vector3, Vector3, ClientProp>
        {

        }
    }
}