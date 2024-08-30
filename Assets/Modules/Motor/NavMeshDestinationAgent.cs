using UnityEngine;
using UnityEngine.AI;

namespace com.playbux.motor
{
    public class NavMeshDestinationAgent : IDestinationAgent
    {
        public Vector3 Position => agent.transform.position;

        private NavMeshAgent agent;

        public NavMeshDestinationAgent(NavMeshAgent agent)
        {
            this.agent = agent;
        }

        public void Cancel()
        {
            this.agent.isStopped = true;
        }

        public void ToDestination(Vector3 destination)
        {
            this.agent.SetDestination(destination);
        }
    }
}