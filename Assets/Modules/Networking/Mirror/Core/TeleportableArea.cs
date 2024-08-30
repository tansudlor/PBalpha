using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.core
{
    public class TeleportableArea : MonoBehaviour
    {
        public bool IsInside => isInside;

        [SerializeField]
        private bool isInside;

        [SerializeField]
        private Collider2D collider2D;

        public bool Validate(Vector2 position)
        {
            bool isOverlapping = collider2D.OverlapPoint(position);
#if UNITY_EDITOR
            Debug.Log($"Is overlapping within teleport area? {isOverlapping}");
#endif
            return isOverlapping;
        }

        public class Factory : PlaceholderFactory<Vector2, TeleportableArea, TeleportableArea>
        {

        }
    }

    public class TeleportableAreaFactory : IFactory<Vector2, TeleportableArea, TeleportableArea>
    {
        private readonly DiContainer container;

        public TeleportableAreaFactory(DiContainer container)
        {
            this.container = container;
        }
        public TeleportableArea Create(Vector2 position, TeleportableArea prefab)
        {
            return container.InstantiatePrefabForComponent<TeleportableArea>(prefab, position, prefab.transform.rotation);
        }
    }
}