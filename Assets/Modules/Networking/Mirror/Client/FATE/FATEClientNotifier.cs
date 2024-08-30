using com.playbux.FATE;
using com.playbux.networking.mirror.message;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.client.FATE
{
    public class FATEClientNotifier : ILateDisposable
    {
        private readonly FATEDatabase fateDatabase;
        private readonly FATENotificationUI.Factory factory;
        private readonly INetworkMessageReceiver<FATENotificationMessage> notificationReceiver;

        private FATENotificationUI notificationUI;

        public FATEClientNotifier(FATEDatabase fateDatabase, FATENotificationUI.Factory factory, INetworkMessageReceiver<FATENotificationMessage> notificationReceiver)
        {
            this.factory = factory;
            this.fateDatabase = fateDatabase;
            this.notificationReceiver = notificationReceiver;
            this.notificationReceiver.OnEventCalled += OnNotificationReceived;
        }

        public void LateDispose()
        {
            notificationReceiver.OnEventCalled -= OnNotificationReceived;
        }

        private void OnNotificationReceived(FATENotificationMessage message)
        {
            var data = fateDatabase.Get(message.fateId);
            
            if (!data.HasValue)
                return;
            
            string remainingTimeText = message.time.hour <= 0 ? "" : message.time.hour + (message.time.hour < 2 ? " hour" : "hours");
            remainingTimeText += " " + message.time.minute + (message.time.minute < 2 ? " minute." : " minutes.");
            notificationUI ??= factory.Create();
            notificationUI.Initialize(remainingTimeText);

#if DEVELOPMENT
            Debug.Log($"F.A.T.E '{data.Value.name}' is about to start in {remainingTimeText} with message: {message.message}");
#endif
        }
    }
}