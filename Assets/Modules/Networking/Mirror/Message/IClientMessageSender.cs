using System;
using Mirror;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public interface IClientMessageSender<T> : ILateTickable where T : struct, NetworkMessage
    {
        event Func<T> Message;
        event Func<bool> SendCondition;
        void Send(T message);
    }

    public interface IServerMessageSender<T> : ILateTickable where T : struct, NetworkMessage
    {
        event Func<T> Message;
        event Func<ServerMessageToObserver<T>> MessageToObserver;
        event Func<bool> SendCondition;
        void Send(T message);
        void Send(bool includeOwner, NetworkIdentity networkIdentity, T message);
    }
}