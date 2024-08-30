using UnityEngine;

namespace com.playbux.sorting
{
    public interface ISortable
    {
        bool IgnoreSorting { get; }
        Vector3 Position { get; }
        void Initialize();
        int GetSortOrder(Vector2 movingObjectPosition);
        Vector2? Distance(Vector2 movingObjectPosition);
    }
}