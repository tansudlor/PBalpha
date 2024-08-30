using System;
using Mirror;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public class ServerNetworkMessageReceiver<T> : IInitializable, ILateDisposable, IServerNetworkMessageReceiver<T> where T : struct, NetworkMessage
    {
        public event Action<NetworkConnectionToClient, T, int> OnEventCalled;

        public void Initialize()
        {
            NetworkServer.RegisterHandler<T>(OnEvent);
        }

        public void LateDispose()
        {
            NetworkServer.UnregisterHandler<T>();
        }

        private void OnEvent(NetworkConnectionToClient connection, T message, int channel)
        {
            OnEventCalled?.Invoke(connection, message, channel);
        }
    }
}
