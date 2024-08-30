#if SERVER
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cysharp.Threading.Tasks;
using Zenject;
using System;
using com.playbux.identity;
using com.playbux.networking.mirror.message;
using System.Linq;
using com.playbux.api;
using com.playbux.firebaseservice;
using Newtonsoft.Json.Linq;

namespace com.playbux.kicktowin
{


    public class KickToWinParticipantData
    {
        public long EnterTick { get; set; }
        public KickToWinParticipantData(long enterTick)
        {
            EnterTick = enterTick;

        }

    }

    public partial class KickToWin
    {
        private IServerNetworkMessageReceiver<KickToWinMessage> kickToWinMessageMessageReceiver;
        private KickToWinMapData kickToWinMapData;
        private CircleScript kickToWinScript;
        private DiContainer diContainer;
        private IIdentitySystem identitySystem;

        private Dictionary<uint, KickToWinParticipantData> participants = new Dictionary<uint, KickToWinParticipantData>();
        private UniqueList<uint> finishPlayer = new UniqueList<uint>();
        private GameObject kickToWinGameObject;
        private int nextIndex;
        private int currentIndex;
        private float bestDistance;

        public KickToWin(KickToWinMapData kickToWinMapData, CircleScript kickToWinScript, DiContainer diContainer, IIdentitySystem identitySystem)
        {
            this.kickToWinMapData = kickToWinMapData;
            this.kickToWinScript = kickToWinScript;
            this.diContainer = diContainer;
            this.identitySystem = identitySystem;
            kickToWinGameObject = kickToWinScript.gameObject;

            KickToWinSpawnPoint().Forget();
            CheckPlayerPosition().Forget();
        }

        async UniTask CheckPlayerPosition()
        {
            while (true)
            {
                await UniTask.Yield();
                foreach (var participant in participants)
                {
                    long stayTime = DateTime.UtcNow.Ticks - participant.Value.EnterTick;
                    long sec = stayTime / 10_000_000;
                    if (sec >= 3)
                    {
                        if (finishPlayer.Contains(participant.Key))
                        {
                            Debug.Log("you get reward already");
                            participant.Value.EnterTick = participant.Value.EnterTick + 60 * 10_000_000;
                            continue;
                        }

                        Debug.Log("you win");
                        try
                        {
                            identitySystem[participant.Key].Identity.connectionToClient.Send(new KickToWinMessage(participant.Key, "youwin", 0, 0, 0));
                            finishPlayer.Add(participant.Key);
                        }
                        catch
                        {
                            participant.Value.EnterTick = participant.Value.EnterTick + 60 * 10_000_000;
                        }

                    }
                }

            }
        }

        async UniTask KickToWinSpawnPoint()
        {
            await UniTask.WaitUntil(() => NetworkServer.active);

            GameObject go = diContainer.InstantiatePrefab(kickToWinGameObject);
            go.transform.position = kickToWinMapData.KickPositionOnWorld[0];
            go.transform.localScale = Vector3.one * 10f;
            var circle = go.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            var script = go.GetComponent<CircleScript>();
            script.OnPlayerExit += OnPlayerExit;
            script.OnPlayerEnter += OnPlayerEnter;
            NetworkServer.Spawn(go);
            nextIndex = 0;
            currentIndex = 0;
            bestDistance = 0;

            rnd();


            while (true)
            {
#if !UNITY_EDITOR
                await UniTask.WaitForSeconds(60f);
#else
                await UniTask.WaitForSeconds(60f);
#endif

                NetworkServer.SendToReady(new KickToWinMessage(0, "closeball", 0, 0, 0));
                NetworkServer.SendToReady(new KickToWinMessage(0, "clearuiall", 0, 0, 0));

                SendScoreToAPI(new List<uint>(finishPlayer)).Forget();
                GameObject.Destroy(go);
                participants.Clear();
                finishPlayer.Clear();
                go = diContainer.InstantiatePrefab(kickToWinGameObject);
                go.transform.position = kickToWinMapData.KickPositionOnWorld[nextIndex];
                go.transform.localScale = Vector3.one * 9f;
                circle = go.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
                script = go.GetComponent<CircleScript>();
                script.OnPlayerExit += OnPlayerExit;
                script.OnPlayerEnter += OnPlayerEnter;

                NetworkServer.Spawn(go);

                await UniTask.WaitForSeconds(0.5f);

                currentIndex = nextIndex;
                bestDistance = 0;

                rnd();

            }

        }

        async UniTask SendScoreToAPI(List<uint> finishPlayerClone)
        {
            if (finishPlayerClone == null)
            { return; }

            if (finishPlayerClone.Count <= 0)
            { return; }

            List<GameResultAPI> gameResultAPIList = new List<GameResultAPI>();
            List<string> scoreToSend = new List<string>();

            for (int i = 0; i < finishPlayerClone.Count; i++)
            {
                uint netid = finishPlayerClone[i];

                GameResultAPI gameResultAPI = new GameResultAPI();

                gameResultAPI.User = identitySystem[netid].UID;
                List<ResultAPI> resultAPIs = new List<ResultAPI>();

                ResultAPI resultAPI = new ResultAPI();
                int currentScore = (finishPlayerClone.Count - i) * FirebaseAuthenticationService.GetInstance().KickToWinMultiplyScore;
                //Debug.Log("currentScore : " + currentScore);
                resultAPI.Value = currentScore.ToString();
                scoreToSend.Add(resultAPI.Value);
                resultAPI.Type = "point";
                resultAPIs.Add(resultAPI);

                gameResultAPI.Results = resultAPIs;
                gameResultAPIList.Add(gameResultAPI);
            }
            Guid guid = Guid.NewGuid();
            string guidToAPI = guid.ToString() + DateTime.UtcNow.Ticks;

            Debug.Log("[KickToWinScore] : " + JsonConvert.SerializeObject(gameResultAPIList));

            (bool isSuccess, JToken reward) = await APIServerConnector.GameAPIResult("kick_to_win", gameResultAPIList, "point", "sum", guidToAPI);

            Dictionary<string, JToken> pebbleMap = new Dictionary<string, JToken>();

            Debug.Log("reward : " + JsonConvert.SerializeObject(reward));

            foreach (var item in reward)
            {
                pebbleMap[item["user"].ToString()] = item;
            }


            if (isSuccess)
            {
                //Debug.Log("[KickToWinScore] isSuccess " + สำเร็จ);
                for (int i = 0; i < finishPlayerClone.Count; i++)
                {
                    uint netid = finishPlayerClone[i];
                    //Debug.Log("[KickToWinScore] " + netid);
                    int score = int.Parse(scoreToSend[i]);
                    //Debug.Log("[KickToWinScore] " + score);
                    identitySystem[netid].Identity.connectionToClient.Send(new KickToWinMessage(netid, "getscore," + guidToAPI, 0, 0, score));
                    string uid = identitySystem[netid].UID;
                    if (pebbleMap.ContainsKey(uid))
                    {
                        try
                        {
                            identitySystem[netid].Identity.connectionToClient.Send(new KickToWinMessage(netid, "getpebble," + guidToAPI, 0, 0, int.Parse(pebbleMap[uid]["quantity"].ToString())));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }



                }

            }
            else
            {
                for (int i = 0; i < finishPlayerClone.Count; i++)
                {
                    uint netid = finishPlayerClone[i];
                    identitySystem[netid].Identity.connectionToClient.Send(new KickToWinMessage(netid, "apierror," + guidToAPI, 0, 0, 0));
                }
            }

            gameResultAPIList.Clear();
            scoreToSend.Clear();

        }



        void OnPlayerEnter(Collider2D collision)
        {

            Debug.Log("Enter : " + collision.gameObject.name + DateTime.UtcNow.ToString());

            try
            {

                uint netId = collision.gameObject.GetComponent<NetworkIdentity>().netId;


                if (finishPlayer.Contains(netId))
                {

                    return;
                }

                long enterTicks = DateTime.UtcNow.Ticks;
                KickToWinParticipantData data = new KickToWinParticipantData(enterTicks);
                long rttTicks = (long)(NetworkTime.rtt / 2) * 10000;
                participants[netId] = data;
                Debug.Log("Enter SendToPlayer : " + collision.gameObject.name + DateTime.UtcNow.ToString());
                identitySystem[netId].Identity.connectionToClient.Send(new KickToWinMessage(netId, "playerenter", enterTicks, enterTicks + rttTicks, 0));

            }
            catch
            {

            }

        }

        void OnPlayerExit(Collider2D collision)
        {
            Debug.Log("Exit : " + collision.gameObject.name + DateTime.UtcNow.ToString());

            try
            {
                uint netId = collision.gameObject.GetComponent<NetworkIdentity>().netId;
                participants.Remove(netId);
                identitySystem[netId].Identity.connectionToClient.Send(new KickToWinMessage(netId, "clearui", 0, 0, 0));

            }
            catch
            {

            }

        }

        public void rnd()
        {
            int randomIndex = UnityEngine.Random.Range(0, kickToWinMapData.KickPositionOnWorld.Length);
            //Debug.Log("randomIndex : " + randomIndex);
            Vector2 randomPos = kickToWinMapData.KickPositionOnWorld[randomIndex];
            Vector2 currentPos = kickToWinMapData.KickPositionOnWorld[currentIndex];
            //Debug.Log("randomPos : " + randomPos);
            // Debug.Log("bestPos : " + bestPos);
            float currentDistance = Vector2.Distance(randomPos, currentPos);
            // Debug.Log("currentDistance : " + currentDistance);
            if (currentDistance > bestDistance)
            {
                nextIndex = randomIndex;
                bestDistance = currentDistance;
                //Debug.Log("bestDistance : " + bestDistance);
            }
        }


    }
}
#endif