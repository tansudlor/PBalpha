using System;
using Mirror;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public class ClientTickMessageSender<T> : IClientMessageSender<T> where T : struct, NetworkMessage
    {
        public event Func<T> Message;
        public event Func<bool> SendCondition;
        
        private uint sendIntervalCounter; 
        
        private double lastSendIntervalTime = double.MinValue;

        private readonly uint sendIntervalMultiplier;
        
        protected ClientTickMessageSender(uint sendIntervalMultiplier)
        {
            this.sendIntervalMultiplier = sendIntervalMultiplier;
        }

        public void Send(T message) => NetworkClient.Send(message);

        public void LateTick()
        {
            TrySend();
        }
        
        private void TrySend()
        {
            if (!NetworkClient.ready)
                return;
            
            if (!SendCondition?.Invoke() ?? true)
            {
                sendIntervalCounter = 0;
                return;
            }

            CheckLastSendTime();
            
            if (sendIntervalCounter != sendIntervalMultiplier)
                return;
            
            if (Message == null)
                return;
            
            Send(Message.Invoke());
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