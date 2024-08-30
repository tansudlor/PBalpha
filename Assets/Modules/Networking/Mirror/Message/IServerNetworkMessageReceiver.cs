using System;
using Mirror;

namespace com.playbux.networking.mirror.message
{
    public interface IServerNetworkMessageReceiver<T> where T : struct, NetworkMessage
    {
        event Action<NetworkConnectionToClient, T, int> OnEventCalled;
    }
}
