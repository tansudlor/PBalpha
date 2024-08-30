using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using com.playbux.firebaseservice;
using com.playbux.api;
using System;
using Zenject;
using com.playbux.identity;
using Mirror;
using Cysharp.Threading.Tasks;
using System.Globalization;
using UnityEngine.Tilemaps;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine.EventSystems;


namespace com.playbux.ui.leaderboard
{
    struct Ranking
    {
        public string eventName;
        public string period;
    }

    class DataAPIRanking
    {
        public JObject rawData;
        public long resendTicks;
        public string apiName;
        public JObject displayData;
    }

    public class LeaderBoardUI : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Sprite clickTabSprite;
        [SerializeField]
        private Sprite normalTabSprite;

        [SerializeField]
        private TMP_Dropdown dropDown;
        [SerializeField]
        private GameObject content;
        [SerializeField]
        private GameObject myScoreNormal;
        [SerializeField]
        private GameObject myScoreC2E;
        [SerializeField]
        private GameObject normalContent;
        [SerializeField]
        private GameObject c2eContent;
        [SerializeField]
        private TextMeshProUGUI endInText;
        [SerializeField]
        private TextMeshProUGUI endInDayText;
        [SerializeField]
        private TextMeshProUGUI dateTimeEndText;
        [SerializeField]
        private TextMeshProUGUI dateTimeSyncText;
        [SerializeField]
        private GameObject emptyGameObject;
        [SerializeField]
        private GameObject scrollView;
        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject quiz_leader_boardGameObject;
        [SerializeField]
        private GameObject headerContent;

        [SerializeField]
        private GameObject headerNormal;
        [SerializeField]
        private GameObject headerC2E;

        [SerializeField]
        private GameObject bringToTopBoard;
        [SerializeField]
        private GameObject bringToTopHeader;



        [SerializeField]
        private SerializedDictionary<string, Sprite> coin = new SerializedDictionary<string, Sprite>();

        private int lastTable = 0;
        private bool noEndDate = false;
        private TimeSpan differenceTime = TimeSpan.Zero;
        private DateTime dateTimeStart = DateTime.MinValue;
        private DateTime dateTimeEnd = DateTime.MinValue;

        private Dictionary<string, object> allSubData = new Dictionary<string, object>();
        private Dictionary<string, object> allPeriodData = new Dictionary<string, object>();
        private Dictionary<string, object> allSyncData = new Dictionary<string, object>();

        private string tabGameObjectName = "";

        private Dictionary<string, string> timeAndData = new Dictionary<string, string>();
        private Dictionary<int, string> numToDate = new Dictionary<int, string>();
        private Dictionary<string, string> dataDateToDate = new Dictionary<string, string>();
        private Dictionary<Ranking, DataAPIRanking> meRanking = new Dictionary<Ranking, DataAPIRanking>();
        private Dictionary<string, string> tableAPI = new Dictionary<string, string>();
        private Dictionary<string, string> pointType = new Dictionary<string, string>();
        private Dictionary<string, bool> headerLeaderboard = new Dictionary<string, bool>();

        private IIdentitySystem identitySystem;
        private UICanvas uiCanvas;
        private string lastTab;
        private string currentPath = "";
#if !SERVER

        [Inject]
        void SetUp(IIdentitySystem identitySystem, UICanvas uiCanvas)
        {
            this.identitySystem = identitySystem;
            this.uiCanvas = uiCanvas;
        }


        void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            dropDown.ClearOptions();
            StartCoroutine(WaitFirebaseData());

        }

        private void Update()
        {

            differenceTime = dateTimeEnd - DateTime.UtcNow;



            if (noEndDate == true)
            {
                endInText.text = "";
                endInDayText.text = "∞";
                return;
            }


            if (dateTimeEnd.Ticks - DateTime.UtcNow.Ticks < 0)
            {
                endInText.text = "Event ended";
            }


            if (differenceTime.Days > 0)
            {
                endInDayText.text = differenceTime.Days.ToString();
                endInText.text = string.Format("{0:00}: {1:00}: {2:00}", differenceTime.Hours, differenceTime.Minutes, differenceTime.Seconds);
            }
            else if (differenceTime.Days == 0)
            {
                endInDayText.text = "0";
                endInText.text = string.Format("{0:00}: {1:00}: {2:00}", differenceTime.Hours, differenceTime.Minutes, differenceTime.Seconds);
            }


        }

        public void OpenLeaderBoard()
        {
            if (canvasGroup.alpha == 0)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                ChangeListData();
                ShowHeader();
            }
            else
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }



        IEnumerator WaitFirebaseData()
        {
            var firebase = FirebaseAuthenticationService.GetInstance();
            yield return new WaitUntil(() => firebase.LeaderboardsCountFinish);
            yield return new WaitUntil(() => firebase.AllSubTable.Count >= firebase.AllSubTableConvertion.Count * firebase.LeaderBoards.Count);
            tableAPI = firebase.TableFromAPI;
            pointType = firebase.PointType;
            allSubData = firebase.AllSubTable;
            allSyncData = firebase.AllSyncTime;
            allPeriodData = firebase.AllPeriodTime;
            dataDateToDate = firebase.AllSubTableConvertion;
            headerLeaderboard = firebase.HeaderLeaderboard;
            
            ShowHeader();

#if TEST_UI
            firebase.OnDataChange = ChangeDropDown;
            firebase.OnHeaderChange = ShowHeader;
            ClickTab(quiz_leader_boardGameObject);
            yield break;
#endif

            yield return new WaitUntil(() => NetworkClient.localPlayer != null);
            yield return new WaitUntil(() => NetworkClient.localPlayer.netId != 0);
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId].UID != null);

            firebase.OnDataChange = ChangeDropDown;
            firebase.OnHeaderChange = ShowHeader;
            ClickTab(quiz_leader_boardGameObject);
        }

        void ShowHeader()
        {
            foreach (var item in headerLeaderboard)
            {
                headerContent.transform.Find(item.Key).gameObject.SetActive(item.Value);
            }
        }

        public void ClickTab(GameObject tab)
        {

           
            headerC2E.SetActive(false);
            headerNormal.SetActive(true);

            GameObject goParent = tab.transform.parent.gameObject;

            Button[] alltab = goParent.transform.GetComponentsInChildren<Button>();

            for (int i = 0; i < alltab.Count(); i++)
            {
                alltab[i].GetComponent<Image>().sprite = normalTabSprite;
                //alltab[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }

            tab.GetComponent<Image>().sprite = clickTabSprite;

            //tab.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1, 191f / 255f, 37f / 255f, 1f);
            tabGameObjectName = tab.name;

            FirebaseAuthenticationService.GetInstance().LastTabLeaderBoard = tabGameObjectName;

            if (tabGameObjectName == "conquer_to_earn_ranking")
            {
                headerNormal.SetActive(false);
                headerC2E.SetActive(true);
            }


            ChangeDropDown(tab.name);

        }


        void ChangeDropDown(string eventName,int change = 0)
        {
            timeAndData.Clear();
            numToDate.Clear();
            dropDown.ClearOptions();
            dropDown.onValueChanged.RemoveAllListeners();

            List<string> optionsData = new List<string>();
            List<string> sessionDataList = new List<string>();
            foreach (var item in allSubData)
            {
                var data = item.Key.Split(".");
                string eventData = data[0];
                string sessionData = data[1];
                //Debug.Log("EVENT DATA:" + eventData);
                if (eventData == eventName)
                {
                    try
                    {
                        //Debug.Log("EVENT DATA " + item.Key + ":" + item.Value);
                        if (item.Value.ToString() == "No Table")
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //item value is null //dont remove this comment 
                        continue;
                    }

                    timeAndData[sessionData] = JsonConvert.SerializeObject(item.Value);
                    optionsData.Add(dataDateToDate[sessionData]);
                    sessionDataList.Add(sessionData);
                }
            }

            for (int i = 0; i < timeAndData.Count; i++)
            {
                numToDate[i] = sessionDataList[i];
            }

            dropDown.AddOptions(optionsData);
            dropDown.onValueChanged.AddListener(ChangeListData);
            ChangeListData(change);
        }

        void ChangeListData(int change = -1)
        {

            scrollRect.verticalNormalizedPosition = 1;

            if (change == -1)
            {
                change = lastTable;
            }

            dropDown.value = change;
          
            if (!numToDate.ContainsKey(change))
            {
                return;
            }


            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            lastTable = change;

            try
            {
                SetDateTime(change);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            string data = timeAndData[numToDate[change]];
            Debug.Log("data   " + data);
            if (data.Replace("\"", "") == "N/A")
            {
                scrollView.SetActive(false);
                myScoreC2E.SetActive(false);
                myScoreNormal.SetActive(false);
                emptyGameObject.SetActive(true);
                return;
            }

            emptyGameObject.SetActive(false);
            scrollView.SetActive(true);

            JArray jsonArray = new JArray();

            jsonArray = JArray.Parse(data);

            Sprite coinSprite = null;

            try
            {
                coinSprite = coin[pointType[tabGameObjectName]];
            }
            catch
            {
                coinSprite = coin["default"];
            }


            GameObject contentShow = null;

            if (tabGameObjectName == "conquer_to_earn_ranking")
            {
                contentShow = c2eContent;
                var playerdata = myScoreC2E.GetComponent<PlayerBarData>();
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    jsonArray[i]["display_name"] = jsonArray[i]["country_data"]["country"];
                    jsonArray[i]["email"] = "";
                    jsonArray[i]["total_score"] = jsonArray[i]["total_pixel_conquer"];
                }
            }
            else
            {
                contentShow = normalContent;
            }

            string localScore = "NA";
            string localRanking = "NA";

            string localUID = "";
            try
            {
                localUID = identitySystem[NetworkClient.localPlayer.netId].UID;
            }
            catch
            {
                localUID = "";
            }

            for (int i = 0; i < jsonArray.Count; i++)
            {
#if !SERVER
                GameObject playerBarPrefab = Instantiate(contentShow, content.transform);

                if (tabGameObjectName == "conquer_to_earn_ranking")
                {
                    var playerBar = playerBarPrefab.GetComponent<PlayerBarData>();
                    playerBar.SetData(jsonArray[i], true);
                    playerBar.SetPlace(i);
                    playerBar.IconSet(jsonArray[i]["display_name"].ToString());
                }
                else
                {
                    var playerBar = playerBarPrefab.GetComponent<PlayerBarData>();
                    playerBar.SetData(jsonArray[i]);
                    if (jsonArray[i]["user_id"].ToString() == localUID)
                    {
                        localScore = jsonArray[i]["total_score"].ToString();
                        localRanking = (i + 1).ToString();
                    }
                    playerBar.SetPlace(i);
                    playerBar.CoinSet(coinSprite);
                }

                playerBarPrefab.SetActive(true);

                if (i > 19)
                {
                    Destroy(playerBarPrefab);
                }
#endif

            }


            try
            {
                MyScoreBar(tabGameObjectName, localScore, localRanking, change, coinSprite);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }



        }


        private void SetDateTime(int change)
        {
            var periodData = GetData(tabGameObjectName, change, allPeriodData);

            JObject periodDataJObject = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(periodData));
            string startDate = periodDataJObject["start"].ToString();
            string endDate = periodDataJObject["end"].ToString();
            string outputFormat = "dd MMM yyyy";

            try
            {
                dateTimeStart = ChangeDateTime(startDate);
                dateTimeEnd = ChangeDateTime(endDate);

                if (dateTimeEnd > DateTime.UtcNow.AddYears(1))
                {
                    startDate = dateTimeStart.ToString(outputFormat, CultureInfo.InvariantCulture);
                    dateTimeEndText.text = startDate;
                    noEndDate = true;
                }
                else
                {

                    startDate = dateTimeStart.ToString(outputFormat, CultureInfo.InvariantCulture);
                    endDate = dateTimeEnd.ToString(outputFormat, CultureInfo.InvariantCulture);

                    dateTimeEndText.text = startDate + " - " + endDate;
                    noEndDate = false;
                }
            }
            catch
            {
                dateTimeStart = ChangeDateTime(startDate);
                startDate = dateTimeStart.ToString(outputFormat, CultureInfo.InvariantCulture);
                dateTimeEndText.text = startDate;
                noEndDate = true;
            }

            /*if (currentPath == "pebble_ranking_leader_board.alpha" || currentPath == "quiz_leader_board.alpha" || currentPath == "kick_to_win_leader_board.alpha")
            {
                dateTimeEndText.text = startDate;
                noEndDate = true;
            }*/


            var syncData = GetData(tabGameObjectName, change, allSyncData);
            DateTime dateTimeUTCSync = ChangeDateTime(syncData.ToString());
            outputFormat = "ddd, dd MMMM yyyy HH:mm:ss 'UTC'";
            string dateTimeSyncDate = dateTimeUTCSync.ToString(outputFormat, CultureInfo.InvariantCulture);
            dateTimeSyncText.text = dateTimeSyncDate;
        }

        private async void MyScoreBar(string tabGameObjectName, string localScore, string localRanking, int change, Sprite coinSprite)
        {
            JObject userDisplay = null;

            if (tabGameObjectName != "conquer_to_earn_ranking")
            {
                userDisplay = await APIData(tabGameObjectName, numToDate[change]);
                if (localScore != "NA")
                {
                    try
                    {

                        userDisplay["ranking"] = int.Parse(localRanking);
                    }
                    catch
                    {
                        userDisplay["ranking"] = 9999;
                    }
                    userDisplay["total_score"] = localScore;
                }

            }

            myScoreC2E.SetActive(false);
            myScoreNormal.SetActive(false);

            if (tabGameObjectName == "conquer_to_earn_ranking")
            {
                myScoreC2E.SetActive(true);

            }
            else
            {
                myScoreNormal.SetActive(true);
                var playerdata = myScoreNormal.GetComponent<PlayerBarData>();
                playerdata.SetData(userDisplay);

                int ranking = userDisplay["ranking"].ToObject<int>();
                Debug.Log("ranking" + ranking);

                if (ranking <= 0)
                {
                    myScoreC2E.SetActive(true);
                    myScoreNormal.SetActive(false);
                    return;
                }

                playerdata.SetPlace(ranking - 1, true);  // 0 1 2 3
                playerdata.CoinSet(coinSprite);

            }
        }

        async UniTask<JObject> APIData(string tabName, string rankingType)
        {
            string gameKey = tableAPI[tabName];

            Ranking ranking = new Ranking();
            ranking.eventName = gameKey;
            ranking.period = rankingType;

            Debug.Log(ranking.eventName + " : " + ranking.period);

            if (meRanking.ContainsKey(ranking))
            {
                if (meRanking[ranking].resendTicks > DateTime.UtcNow.Ticks) //resendTicks > Now.Ticls return old displaydata
                {
                    Debug.Log("HaveData");
                    return meRanking[ranking].displayData;
                }

            }
            Debug.Log("DontHaveData");
            var allData = await APIServerConnector.MeRanking(gameKey, rankingType, PlayerPrefs.GetString(TokenUtility.accessTokenKey));
            JObject rawData = allData.Item1;
            string apiName = allData.Item2;

            //Debug.Log("rawData " + JsonConvert.SerializeObject(rawData));

            JObject displayData = new JObject();
            displayData["ranking"] = rawData["data"]["ranking"];
            displayData["display_name"] = identitySystem[NetworkClient.localPlayer.netId].DisplayName;
            displayData["email"] = identitySystem[NetworkClient.localPlayer.netId].Email;
            displayData["total_score"] = rawData["data"]["total_score"];


            DataAPIRanking dataAPIRanking = new DataAPIRanking();
            dataAPIRanking.rawData = rawData;
            dataAPIRanking.apiName = apiName;
            dataAPIRanking.resendTicks = DateTime.UtcNow.Ticks + 1_200_000_000;
            dataAPIRanking.displayData = displayData;

            meRanking[ranking] = dataAPIRanking;

            Debug.Log("displayData " + JsonConvert.SerializeObject(meRanking[ranking].displayData));

            return meRanking[ranking].displayData;

        }


        object GetData(string leaderboardName, int dropdownNum, Dictionary<string, object> dictData)
        {

            string path = leaderboardName + "." + numToDate[dropdownNum];

            currentPath = path;

            object data = null;
            foreach (var item in dictData)
            {
                var dataPath = item.Key;
                if (dataPath == path)
                {
                    data = item.Value;
                }

            }

            return data;

        }

        DateTime ChangeDateTime(string date)
        {
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime dateTime = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            return dateTime;
        }

#endif
        }
}