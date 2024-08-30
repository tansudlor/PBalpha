using System.Collections.Generic;
using com.playbux.servercollection;

namespace com.playbux.gameeventcollection
{
    public enum GameEventState
    {
        None,
        Prepare,
        Play,
        End
    }


    

    public class GameEventCollection : ServerCollection<IGameEvent, GameEventState>
    {
        public override GameEventState GetState(IGameEvent eventName)
        {
            if (!datas.ContainsKey(eventName))
            {
                return GameEventState.None;
            }

            return datas[eventName];
        }

        public override void Clean()
        {
            Dictionary<IGameEvent, GameEventState> cleanCollection = new Dictionary<IGameEvent, GameEventState>();  
            foreach (var data in datas)
            {
                if(data.Value != GameEventState.End)
                {
                    cleanCollection[data.Key] = data.Value;
                }
            }
            datas.Clear();
            datas = cleanCollection;
        }
    }
}