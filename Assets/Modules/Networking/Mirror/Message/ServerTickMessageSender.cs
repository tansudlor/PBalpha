using System;
using Mirror;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public struct ServerMessageToObserver<T> : IEquatable<ServerMessageToObserver<T>> where T : struct, NetworkMessage
    {
        public readonly bool IncludeOwner;
        public readonly NetworkIdentity NetworkIdentity;
        public readonly T Message;

        public ServerMessageToObserver(bool includeOwner, NetworkIdentity networkIdentity, T message)
        {
            Message = message;
            IncludeOwner = includeOwner;
            NetworkIdentity = networkIdentity;
        }
        
        public bool Equals(ServerMessageToObserver<T> other)
        {
            return IncludeOwner == other.IncludeOwner && Equals(NetworkIdentity, other.NetworkIdentity) && Message.Equals(other.Message);
        }

        public override bool Equals(object obj)
        {
            return obj is ServerMessageToObserver<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IncludeOwner, NetworkIdentity, Message);
        }
    }
    
    public class ServerTickMessageSender<T> : IServerMessageSender<T> where T : struct, NetworkMessage
    {
        public event Func<bool> SendCondition;
        public event Func<T> Message;
        public event Func<ServerMessageToObserver<T>> MessageToObserver;
        
        private uint sendIntervalCounter; 
        
        private double lastSendIntervalTime = double.MinValue;

        private readonly uint sendIntervalMultiplier;
        
        protected ServerTickMessageSender(uint sendIntervalMultiplier)
        {
            this.sendIntervalMultiplier = sendIntervalMultiplier;
        }

        public void LateTick()
        {
            TrySend();
        }

        public void Send(T message)
        {
            NetworkServer.SendToReady(message);
        }

        public void Send(bool includeOwner, NetworkIdentity networkIdentity, T message)
        {
            NetworkServer.SendToReadyObservers(networkIdentity, message, includeOwner);
        }
        
        private void TrySend()
        {
            if (!NetworkServer.active)
                return;
            
            if (!SendCondition?.Invoke() ?? true)
            {
                sendIntervalCounter = 0;
                return;
            }

            CheckLastSendTime();
            
            if (sendIntervalCounter != sendIntervalMultiplier)
                return;
            
            if (Message != null)
                Send(Message.Invoke());

            if (MessageToObserver != null)
            {
                var func = MessageToObserver.Invoke();
                Send(func.IncludeOwner, func.NetworkIdentity, func.Message);
            }
        }

        private void CheckLastSendTime()
        {
            if (sendIntervalCounter == sendIntervalMultiplier)
                sendIntervalCounter = 0;
            
            if (AccurateInterval.Elapsed(NetworkTime.localTime, NetworkServer.sendInterval, ref lastSendIntervalTime))
                sendIntervalCounter++;
        }
    }
}