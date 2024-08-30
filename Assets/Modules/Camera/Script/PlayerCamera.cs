using Cinemachine;
using UnityEngine;

namespace com.playbux.Camera
{
    public class PlayerCamera : ICamera
    {
        private readonly Transform transform;
        private readonly ICinemachineCamera camera;
        public PlayerCamera(Transform transform, ICinemachineCamera camera)
        {
            this.camera = camera;
            this.transform = transform;
        }
        
        public void Engage()
        {
            camera.VirtualCameraGameObject.SetActive(true);
        }

        public void Disengage()
        {
            camera.VirtualCameraGameObject.SetActive(false);
        }
        
        public void Follow(Transform other)
        {
            transform.position = new Vector3(other.position.x, 0, other.position.z);
            camera.Follow = other;
        }
    }
}