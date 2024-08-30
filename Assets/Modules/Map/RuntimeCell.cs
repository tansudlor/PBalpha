using UnityEngine;
using Zenject;

namespace com.playbux.map
{
    public class RuntimeCell
    {
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;

        private readonly Transform transform;
        private readonly GameObject gameObject;
        private readonly SpriteRenderer renderer;

        public RuntimeCell(Transform transform, GameObject gameObject, SpriteRenderer renderer)
        {
            this.transform = transform;
            this.gameObject = gameObject;
            this.renderer = renderer;
        }

        public void Initialize(Vector3 position, Sprite sprite)
        {
            transform.position = position;
            renderer.sprite = sprite;
            gameObject.SetActive(true);
        }

        public void Dispose()
        {
            renderer.sprite = null;
            gameObject.SetActive(false);
        }

        public class Pool : MemoryPool<Vector3, Sprite, RuntimeCell>
        {
            protected override void Reinitialize(Vector3 p1, Sprite p2, RuntimeCell item)
            {
                base.Reinitialize(p1, p2, item);
                item.Initialize(p1, p2);
            }

            protected override void OnDespawned(RuntimeCell item)
            {
                item.Dispose();
                base.OnDespawned(item);
            }

            protected override void OnDestroyed(RuntimeCell item)
            {
                item.Dispose();
                base.OnDestroyed(item);
            }
        }
    }
}