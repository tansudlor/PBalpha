using com.playbux.networking.mirror.client.prop;
using com.playbux.sorting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.npc
{
    public class NPCSorter : MonoBehaviour
    {
        public ISortable Sortable => sortable;

        private ISortable sortable;

        [Inject]
        private void Setup(ISortable sortable)
        {
            this.sortable = sortable;
            this.sortable.Initialize();
        }

        public class Factory : PlaceholderFactory<GameObject, Vector3, NPCSorter>
        {

        }
    }
}