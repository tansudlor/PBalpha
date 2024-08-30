using UnityEngine;

namespace com.playbux.motor
{
    public interface IMotor
    {
        float MoveSpeed { get; }
        float Acceleration { get; }
        Vector3 Position { get; }

        void Initialize();

        void Dispose();
        
        void Stop();
        void Move(Vector3 v1);
    }
}