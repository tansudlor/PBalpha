using Zenject;

namespace com.playbux.networking.mirror.client
{
    public interface IClientBehaviour
    {
        void Initialize();
        void Dispose();

        public class Factory : PlaceholderFactory<bool, IClientBehaviour>
        {

        }
    }

    public class PlayerClientBehaviourFactory : IFactory<bool, IClientBehaviour>
    {
        private readonly DiContainer container;

        public PlayerClientBehaviourFactory(DiContainer container)
        {
            this.container = container;
        }

        public IClientBehaviour Create(bool isOwn)
        {
            IClientBehaviour behaviour = isOwn ? container.Instantiate<OwnPlayerClientBehaviour>() : container.Instantiate<OtherPlayerClientBehaviour>();
            behaviour.Initialize();
            return behaviour;
        }
    }
}