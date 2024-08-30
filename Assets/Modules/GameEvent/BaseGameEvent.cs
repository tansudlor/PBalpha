
using Cysharp.Threading.Tasks;
using UnityEngine;
using com.playbux.gameeventcollection;
using Mirror;

namespace com.playbux.gameevent
{
   
    public abstract class BaseGameEvent<TValue> :MonoBehaviour, IGameEvent
    {
        
        protected long endPrepare;
        public long EndPrepare { get =>endPrepare;}
        public abstract void SetJobDiscription(TValue desc);
        public abstract UniTaskVoid RunEvent();
        public abstract void CloseEvent();

        public abstract void OnClientConnected(NetworkConnectionToClient connection);
      
    }
    
}
