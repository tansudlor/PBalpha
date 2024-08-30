using Zenject;
using UnityEngine;
using com.playbux.action;

namespace com.playbux.networking.mirror.client.action
{
    public abstract class ClientActionArea : MonoBehaviour, IActionArea
    {
        public int UUID => gameObject.GetHashCode();
        public bool NeedInteractButton => needInteractButton;

        internal Collider2D Collider => collider;

        [SerializeField]
        private bool needInteractButton;

        [SerializeField]
        private Collider2D collider;

        public void Initialize()
        {

        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }

        public abstract void OnAreaEnter();
        public virtual bool Validate(Vector3 position)
        {
            return collider.OverlapPoint(position);
        }
    }
}