using UnityEngine;

namespace com.playbux.motor
{
    public class DestinationMotor : IMotor
    {
        public float MoveSpeed { get; }
        public float Acceleration { get; }
        public Vector3 Position => agent.Position;

        private IDestinationAgent agent;

        public DestinationMotor(IDestinationAgent agent)
        {
            this.agent = agent;
        }
        
        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        public void Stop()
        {
            agent.Cancel();
        }

        public void Move(Vector3 destination)
        {
            agent.ToDestination(destination);
        }
    }
}