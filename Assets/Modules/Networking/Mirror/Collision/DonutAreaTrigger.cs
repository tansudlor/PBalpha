using UnityEngine;
namespace com.playbux.networking.mirror.collision
{
    public class DonutAreaTrigger : MonoBehaviour, ITrigger<Vector2>
    {
        [SerializeField]
        private CircleCollider2D innerRing;

        [SerializeField]
        private CircleCollider2D outerRing;

        public bool IsTrigger(Vector2 other)
        {
            return outerRing.OverlapPoint(other) && !innerRing.OverlapPoint(other);
        }

        public void SetActive(bool enabled)
        {
            innerRing.enabled = enabled;
        }
    }
}