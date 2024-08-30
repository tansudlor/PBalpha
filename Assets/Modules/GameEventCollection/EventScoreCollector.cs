#if SERVER
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

namespace com.playbux.gameeventcollection
{
    public class EventScoreCollector : IEventScore
    {
        public struct PlayerAndEvent
        {
            public string EventName;
            public string UID;

            public PlayerAndEvent(string eventName, string uid)
            {
                EventName = eventName;
                UID = uid;
            }
        }

        public Dictionary<PlayerAndEvent, uint> PlayerEventScore = new Dictionary<PlayerAndEvent, uint>();

        public async UniTask SendScore(string uid, string eventname, uint score)
        {
            await UniTask.Delay(1);
            var key = new PlayerAndEvent(eventname, uid);

            PlayerEventScore[key] = score;

        }

        public async UniTask<uint> GetScore(string uid, string eventname)
        {
            await UniTask.Delay(1);

            var key = new PlayerAndEvent(eventname, uid);
            if (!PlayerEventScore.ContainsKey(key))
            {
                PlayerEventScore[key] = 0;
            }
            

            return PlayerEventScore[key];
            
        }

    }
}
#endif