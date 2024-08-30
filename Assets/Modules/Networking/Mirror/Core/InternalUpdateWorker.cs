using System;
using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror
{
    public class InternalUpdateWorker : ITickable
    {
        public event Action OnTick;
        public event Action OnUpdateLoop;

        private bool isInitialized;
        private uint tickInterval;
        private uint tickIntervalMultiplier = 1;
        private double lastSendIntervalTime;
        
        private const double TICK_INTERVAL = 1D / 60D;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void Dispose()
        {
            isInitialized = false;
        }

        public void Tick()
        {
            if (!isInitialized)
                return;

            OnUpdateLoop?.Invoke();
            
            if (AccurateInterval.Elapsed(NetworkTime.localTime, TICK_INTERVAL, ref lastSendIntervalTime))
            {
                if (tickInterval == tickIntervalMultiplier)
                {
                    tickInterval = 0;
                    OnTick?.Invoke();
                }
                
                tickInterval++;
            }
        }
    }

    public class NetworkUpdateWorker : ITickable
    {
        public event Action OnTick;

        private bool isInitialized;
        private uint tickInterval;
        private uint tickIntervalMultiplier = 1;
        private double lastSendIntervalTime;

        private double tickInternal = NetworkServer.sendInterval;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void Dispose()
        {
            isInitialized = false;
        }

        public void Tick()
        {
            if (!isInitialized)
                return;

            if (AccurateInterval.Elapsed(NetworkTime.localTime, tickInternal, ref lastSendIntervalTime))
            {
                if (tickInterval == tickIntervalMultiplier)
                {
                    tickInterval = 0;
                    OnTick?.Invoke();
                }
                
                tickInterval++;
            }
        }
    }
}