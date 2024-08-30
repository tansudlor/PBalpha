using System;
using Zenject;
using UnityEngine;
using System.Collections.Generic;
using com.playbux.network;
using com.playbux.network.api.rest;
using Cysharp.Threading.Tasks;
using NanoidDotNet;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace com.playbux.networking.server.stepcount
{
    [Serializable]
    public class WalkToWinResponseData
    {
        public WalkToWinResponseSession gameSession;
    }

    [Serializable]
    public class WalkToWinResponseSession
    {
        public string game_key;
        public string session_key;
    }

    [Serializable]
    public class WalkToWinSendApiData
    {
        public string game_key;
        public WalkToWinResultCollection[] game_results;
        public string result_type;
        public string result_operator;
        public string session_key;

        public WalkToWinSendApiData(string gameKey, WalkToWinResultCollection[] gameResults, string resultType, string resultOperator)
        {
            game_key = gameKey;
            game_results = gameResults;
            result_type = resultType;
            result_operator = resultOperator;
            session_key = DateTime.UtcNow.ToShortDateString() + Nanoid.Generate("_-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 5);
        }
    }

    [Serializable]
    public class WalkToWinResultCollection
    {
        public string user;
        public WalkToWinResult[] results;

        public WalkToWinResultCollection(string user, WalkToWinResult[] results)
        {
            this.user = user;
            this.results = results;
        }
    }

    [Serializable]
    public class WalkToWinResult
    {
        public string value;
        public string type;
    }

    public class StepCounter : IInitializable, ILateTickable
    {
        private const string GAME_KEY = "buxathon";
        private const string GAME_STEP_SEGMENT = "operations/game-results";

        private readonly IDataStore dataStore;

        private long currentDateTime;
        private long scheduleDateTime;
        private Dictionary<string, float> userSteps = new Dictionary<string, float>();

        public StepCounter(PackApiFacade packApi)
        {
            dataStore = packApi.GetDataStore(DomainEnum.GameBackend);
        }

        public void Initialize()
        {
            Reschedule();
            SendTest().Forget();




        }



        public void LateTick()
        {
            /*currentDateTime = DateTime.UtcNow.Ticks;

            if (scheduleDateTime > currentDateTime)
                return;

            Send().Forget();
            Reschedule();*/
        }

        public void Count(string uid)
        {
            userSteps.TryAdd(uid, 0);
            userSteps[uid] += 1 / 12f;

#if UNITY_EDITOR
            // Debug.Log($"<color=green>[Step Counter System]</color>: <color=cyan>{uid}</color> has walk 1 more step, totaling [<color=yellow>{Mathf.RoundToInt(userSteps[uid])}</color>]");
#endif
        }

        private async UniTaskVoid SendTest()
        {

            while (true)
            {

                await UniTask.WaitForSeconds(600f);
                Send().Forget();

            }

        }


        private async UniTaskVoid Send()
        {
            List<WalkToWinResultCollection> collections = new List<WalkToWinResultCollection>();
            foreach (var pair in userSteps)
            {
                //TODO: sending user steps
                var results = new WalkToWinResult[1];
                results[0] = new WalkToWinResult
                {
                    type = "point",
                    value = Mathf.RoundToInt(pair.Value).ToString()
                };
                collections.Add(new WalkToWinResultCollection(pair.Key, results));
                Debug.Log($"<color=green>[Step Counter System]</color>: Send <color=cyan>{Mathf.RoundToInt(pair.Value)}</color> step(s) of [<color=yellow>{pair.Key}</color>]");
            }

            var body = new WalkToWinSendApiData(GAME_KEY, collections.ToArray(), "point", "sum");
            var response = await dataStore.Post<WalkToWinResponseData>(body, GAME_STEP_SEGMENT);

            if (response.errorMessage.statusCode != 0)
                Debug.LogWarning($"{response.errorMessage.statusCode}:{response.errorMessage.error} with message {response.errorMessage.message}");
            else
            {
                if (response.data.gameSession.session_key != body.session_key)
                    Debug.LogWarning($"<color=green>[Step Counter System]</color> Warning: Session key is invalid, sent [{body.session_key}] response [{response.data.gameSession.session_key}]");

                if (response.data.gameSession.game_key != body.game_key)
                    Debug.LogWarning($"<color=green>[Step Counter System]</color> Warning: Game key is invalid, sent [{body.game_key}] response [{response.data.gameSession.game_key}]");
            }

            userSteps.Clear();
        }

        private void Reschedule()
        {
            var now = DateTime.UtcNow;
            scheduleDateTime = new DateTime(now.Year, now.Month, now.Day).AddHours(7).Ticks;

#if DEVELOPMENT
            Debug.Log($"<color=green>[Step Counter System]</color>: Server has started at <color=cyan>{now.ToShortTimeString()}</color>, schedule sending time is/was at <color=cyan>{new DateTime(scheduleDateTime).ToShortTimeString()}</color>");
#endif

            if (scheduleDateTime > DateTime.UtcNow.Ticks)
                return;

            now = now.AddDays(1);
            scheduleDateTime = new DateTime(now.Year, now.Month, now.Day).AddHours(7).Ticks;

#if DEVELOPMENT
            Debug.Log($"<color=green>[Step Counter System]</color>: Reschedule for tomorrow at <color=cyan>{new DateTime(scheduleDateTime).ToShortTimeString()}</color> UTC because server has started at <color=cyan>{now.ToShortTimeString()}</color> past the sending time");
#endif
        }
    }
}