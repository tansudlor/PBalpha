using System;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace SETHD.Utilities
{
    #nullable enable
    public class Invoker : MonoBehaviour
    {
        [Serializable]
        public enum InvokeCycle
        {
            Start,
            FrameOne,
            Custom = 0
        }

        private bool hasInvoked;
        
        [SerializeField]
        private float delay = 0;
        
        [SerializeField]
        private InvokeCycle cycle;

        [SerializeField]
        private UnityEvent events;

        private async UniTaskVoid Start()
        {
            if (cycle != InvokeCycle.Start)
                return;

            await UniTask.Delay(Mathf.CeilToInt(1000 * delay), DelayType.DeltaTime);
            events?.Invoke();
        }
        
        private async UniTaskVoid Update()
        {
            if (hasInvoked)
                return;
            
            if (cycle != InvokeCycle.FrameOne)
                return;

            await UniTask.DelayFrame(1);
            await UniTask.Delay(Mathf.CeilToInt(1000 * delay), DelayType.DeltaTime);
            events?.Invoke();
            hasInvoked = true;
        }
    }
}