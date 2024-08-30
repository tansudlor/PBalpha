using com.playbux.api;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Zenject;

namespace com.playbux.kicktowin
{
    public class KickToWinUI : MonoBehaviour
    {

        [SerializeField]
        private RectTransform scoreBarRect;
        [SerializeField]
        private TextMeshProUGUI scoreText;
        [SerializeField]
        private GameObject roundScorePrefab;
        [SerializeField]
        private GameObject spawnPoint;

        private bool isOpen = false;
        private Vector3 final = Vector3.zero;

        private IIdentitySystem identitySystem;

        private int score = 0;
        private int roundScore = 0;

        

#if !SERVER

        [Inject]
        void SetUp(IIdentitySystem identitySystem)
        {
            //Debug.Log("BugGen0");
            this.identitySystem = identitySystem;
        }


        private void Start()
        {
            //Debug.Log("BugGen");
            StartCoroutine(WaitForUID());
        }

        IEnumerator WaitForUID()
        {
            var scale = transform.localScale;
            transform.localScale = Vector3.zero;
            final = new Vector3(-146.86f, -0f, 0f);
            scoreBarRect.localPosition = new Vector3(-146.86f, -0f, 0f);
            yield return new WaitWhile(() => NetworkClient.localPlayer == null);
            yield return new WaitUntil(() => identitySystem.ContainsKey(NetworkClient.localPlayer.netId));
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId] != null);
            transform.localScale = scale;
            APIData().Forget();

        }

        async UniTask APIData()
        {
            
            var allData = await APIServerConnector.MeRanking("kick_to_win", "alpha", PlayerPrefs.GetString(TokenUtility.accessTokenKey));
            JObject rawData = allData.Item1;
            string apiName = allData.Item2;
            Debug.Log("rawData " + JsonConvert.SerializeObject(rawData));
            int totalScore = rawData["data"]["total_score"].ToObject<int>();
            score = totalScore;
            scoreText.text = score.ToString("N0");
        }



        private void Update()
        {
            scoreBarRect.localPosition += (final - scoreBarRect.localPosition) / 4.0f;
            OpenKickToWinUI();
        }

        public void On()
        {
            isOpen = true;
        }
        public void Off()
        {
            isOpen = false;
        }

        public void Toggle()
        {
            isOpen = !isOpen;
        }

        public void OpenKickToWinUI()
        {
            if (isOpen)
            {
                final = new Vector3(0f, -0f, 0f);

            }
            else
            {
                final = new Vector3(-146.86f, -0f, 0f);
            }
        }

        public void ScoreUp(int currectScore)
        {
            this.score += 1;
            scoreText.text = this.score.ToString("N0");
           
        }

        public void RoundScoreUp(int currectScore)
        {
            
            var roundScoreGameObject = Instantiate(roundScorePrefab, spawnPoint.transform);
            roundScoreGameObject.GetComponent<RoundScoreText>().Score = currectScore + 1;
        }

#endif
    }
}
