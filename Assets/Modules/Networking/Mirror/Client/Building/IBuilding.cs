using com.playbux.sorting;
using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.client.building
{
    public interface IBuilding
    {
        Vector3 Position { get; }
        ISortable Sortable { get; }
        void Initialize();
        void Hide();
        void Show();
        void Dispose();

        public class Factory : PlaceholderFactory<GameObject, Vector3, IBuilding>
        {

        }
    }
}