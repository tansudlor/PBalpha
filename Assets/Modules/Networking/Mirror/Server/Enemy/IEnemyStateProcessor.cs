using com.playbux.enemy;

namespace com.playbux.networking.mirror.server.enemy
{
    public interface IEnemyStateProcessor
    {
        void Start();
        void Stop();
        void ChangeState(EnemyState toState);
    }
}