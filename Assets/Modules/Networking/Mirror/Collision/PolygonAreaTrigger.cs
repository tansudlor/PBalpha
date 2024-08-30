using UnityEngine;

namespace com.playbux.networking.mirror.collision
{
    public class PolygonAreaTrigger : MonoBehaviour, ITrigger<Vector2>
    {
        [SerializeField]
        private PolygonCollider2D collider;

        public bool IsTrigger(Vector2 other)
        {
            return collider.OverlapPoint(other);
        }

        public void SetActive(bool enabled)
        {
            collider.enabled = enabled;
        }
    }
}