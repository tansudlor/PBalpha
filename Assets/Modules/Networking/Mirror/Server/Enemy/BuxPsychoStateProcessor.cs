using com.playbux.enemy;
using System.Collections.Generic;

namespace com.playbux.networking.mirror.server.enemy
{
    public class BuxPsychoStateProcessor : IEnemyStateProcessor
    {
        private readonly Dictionary<EnemyState, IEnemyServerBehaviour> states;

        private EnemyState currentState;

        public void Start()
        {

        }
        public void Stop()
        {

        }
        public void ChangeState(EnemyState toState)
        {

        }
    }
}