using System;
using UnityEngine;

namespace com.playbux.networking.mirror.client
{
    public interface IAFKWorker
    {
        bool IsAFK { get; }
        event Action OnAFK;
        void Initialize();
        void Dispose();
        void PerformATK();
    }

    public class AFKWorker : IAFKWorker
    {
        public bool IsAFK { get; }
        public event Action OnAFK;

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        public void PerformATK()
        {
            
        }
    }
}