using com.playbux.ui.sortable;
using UnityEngine.EventSystems;

namespace com.playbux.ui.draggable
{
    public interface IDraggableUI : IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        void SetLock(bool isLock);
    }
}