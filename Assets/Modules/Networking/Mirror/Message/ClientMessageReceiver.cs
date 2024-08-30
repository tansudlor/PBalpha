using System;
using com.playbux.events;
using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public class ClientMessageReceiver<T> : INetworkMessageReceiver<T>, IInitializable, ILateDisposable where T : struct, NetworkMessage
    {
        public event Action<T> OnEventCalled;

        private bool isSubscribed;
        private SignalBus signalBus;

        public ClientMessageReceiver(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            this.signalBus.Subscribe<LoginSignal>(Subscribe);
            this.signalBus.Subscribe<LogoffSignal>(Unsubscribe);
        }

        public void Initialize()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (isSubscribed)
                return;

            NetworkClient.RegisterHandler<T>(OnEvent);

#if DEVELOPMENT
            Debug.Log($"Subscribed message {typeof(T).Name} on client");
#endif

            isSubscribed = true;
        }

        private void Unsubscribe(LogoffSignal signal)
        {
            NetworkClient.UnregisterHandler<T>();
            isSubscribed = false;
        }

        public void LateDispose()
        {
            NetworkClient.UnregisterHandler<T>();
            signalBus.Unsubscribe<LoginSignal>(Subscribe);
            signalBus.Unsubscribe<LogoffSignal>(Unsubscribe);
        }

        private void OnEvent(T message)
        {
            OnEventCalled?.Invoke(message);
        }
    }
}
