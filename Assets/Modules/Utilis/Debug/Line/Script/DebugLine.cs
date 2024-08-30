using Zenject;
using UnityEngine;

namespace com.playbux.utilis.debug.line
{
    public class DebugLine : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer renderer;


        private void Initialize(float thickness, Vector3 startPosition, Vector3 endPosition, Transform parent)
        {
            transform.position = (startPosition + endPosition) * 0.5f;
            renderer.startWidth = thickness;
            renderer.endWidth = thickness;
            renderer.SetPosition(0, startPosition);
            renderer.SetPosition(1, endPosition);
            transform.SetParent(parent);
        }

        public class Pool : MonoMemoryPool<float, Vector3, Vector3, Transform, DebugLine>
        {
            protected override void Reinitialize(float thickness, Vector3 startPosition, Vector3 endPosition, Transform parent, DebugLine debugLine)
            {
                debugLine.Initialize(thickness, startPosition, endPosition, parent);
            }
        }
    }
}