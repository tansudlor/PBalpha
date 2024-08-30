using Zenject;
using UnityEngine;

namespace com.playbux.motor
{
    public class SteerMotor : IMotor
    {
        public float MoveSpeed => moveSpeed;
        public float Acceleration => 1;
        public Vector3 Position => transform.position;

        private bool isInitialized;
        private float moveSpeed = 0.270f;

        private Vector3 direction;
        private Transform transform;

        public SteerMotor(Transform transform)
        {
            direction = Vector3.zero;
            this.transform = transform;
        }

        public void Initialize()
        {
            isInitialized = true;
        }

        public void Dispose()
        {
            isInitialized = false;
        }

        public void Stop()
        {

        }

        public void Move(Vector3 direction)
        {
            if (!isInitialized)
                return;

            transform.position += direction * moveSpeed;
        }
    }
}