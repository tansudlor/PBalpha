using System;
using UnityEngine.Events;

namespace com.playbux.functioncollection
{
    [Serializable]
    public struct FunctionMap
    {
        public string FunctionKey;
        public UnityEvent Called;
    }
}
