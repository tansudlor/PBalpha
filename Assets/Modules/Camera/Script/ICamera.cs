using UnityEngine;

namespace com.playbux.Camera
{
    public interface ICamera
    {
        void Engage();

        void Disengage();

        void Follow(Transform other);
    }
}

