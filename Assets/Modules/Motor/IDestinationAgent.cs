using UnityEngine;

namespace com.playbux.motor
{
    public interface IDestinationAgent
    {
        Vector3 Position { get; }
        void Cancel();
        void ToDestination(Vector3 destination);
    }
}