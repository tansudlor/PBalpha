using UnityEngine;
namespace com.playbux.networking.mirror.collision
{
    public class CircleAreaTrigger : MonoBehaviour, ITrigger<Vector2>
    {
        [SerializeField]
        private CircleCollider2D collider;

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