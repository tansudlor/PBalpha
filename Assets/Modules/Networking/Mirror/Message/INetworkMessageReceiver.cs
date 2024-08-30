using System;
using Mirror;

namespace com.playbux.networking.mirror.message
{
    public interface INetworkMessageReceiver<T> where T : struct, NetworkMessage
    {
        event Action<T> OnEventCalled;
    }
}
