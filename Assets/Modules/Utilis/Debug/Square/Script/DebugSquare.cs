using Zenject;
using UnityEngine;

namespace com.playbux.utilis
{
    public class DebugSquare : MonoBehaviour
    {
        private void Initialize(float scale, Vector3 position, Transform parent)
        {
            transform.position = position;
            transform.localScale = transform.lossyScale * (1 + scale);
            transform.SetParent(parent);
        }
        public class Pool : MonoMemoryPool<float, Vector3, Transform, DebugSquare>
        {
            protected override void Reinitialize(float scale, Vector3 position, Transform parent, DebugSquare debugSquare)
            {
                debugSquare.Initialize(scale, position, parent);
            }
        }
    }
}