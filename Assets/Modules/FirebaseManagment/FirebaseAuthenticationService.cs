using Firebase.Auth;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Firebase.Database;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using System.Data.SqlClient;

namespace com.playbux.firebaseservice
{
    public class UniqueList<T> : List<T>
    {
        private HashSet<T> values = new HashSet<T>();
        public virtual new void Add(T value)
        {
            if (!values.Contains(value))
            {
                values.Add(value);
                base.Add(value);
            }
        }
        public virtual new void Clear()
        {
            values.Clear();
            base.Clear();
        }

        public virtual new void Remove(T value)
        {
            if (values.Contains(value))
            {
                values.Remove(value);
                base.Remove(value);
            }
        }

    }

    public class FirebaseAuthenticationService
    {

        private static FirebaseAuthenticationService instance;
        private WelcomeBoard welcomeBoard;
        private PlaybuxNews playbuxNews;
        private HashSet<string> subTableList = new HashSet<string>();
        private Dictionary<string, object> allSubTable = new Dictionary<string, object>();
        private Dictionary<string, object> leaderBoards = new Dictionary<string, object>();
        private Dictionary<string, string> allSubTableConvertion = new Dictionary<string, string>();
        private Dictionary<string, string> tableFromAPI = new Dictionary<string, string>();
        private Dictionary<string, object> allSyncTime = new Dictionary<string, object>();
        private Dictionary<string, object> allPeriodTime = new Dictionary<string, object>();
        private Dictionary<string, string> pointType = new Dictionary<string, string>();
        private Dictionary<string, bool> headerLeaderboard = new Dictionary<string, bool>();
        private Dictionary<string, string> playbuxNewsDict = new Dictionary<string, string>();
        private string lastTabLeaderBoard = "";

        private int kickToWinMultiplyScore;
        string userId;
        string token = "";
        string url = "https://playbux-unity-default-rtdb.asia-southeast1.firebasedatabase.app/staging/client_setting.json";
        string urlPut = "https://playbux-unity-default-rtdb.asia-southeast1.firebasedatabase.app/staging/client_setting/Test.json";

        public static string devAPIPath = "";
        public bool LeaderboardsCountFinish = false;
        private string environment = "staging";

        private string welcomeBoardImage;

#if !PRODUCTION
        string urlAPIFailData = "https://playbux-unity-default-rtdb.asia-southeast1.firebasedatabase.app/staging/game_server_api_caching.json";
#endif

#if PRODUCTION
        string urlAPIFailData = "https://playbux-unity-default-rtdb.asia-southeast1.firebasedatabase.app/production/game_server_api_caching.json";
#endif

#if DEVELOPMENT_API
        string urlAPIFailData = "https://playbux-unity-default-rtdb.asia-southeast1.firebasedatabase.app/dev/game_server_api_caching";
#endif

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        private JObject clientSettingData;

        public JObject ClientSettingData { get => clientSettingData; set => clientSettingData = value; }
        public string WelcomeBoardImage { get => welcomeBoardImage; set => welcomeBoardImage = value; }
        public Dictionary<string, object> AllSubTable { get => allSubTable; set => allSubTable = value; }
        public int KickToWinMultiplyScore { get => kickToWinMultiplyScore; set => kickToWinMultiplyScore = value; }
        public Dictionary<string, string> AllSubTableConvertion { get => allSubTableConvertion; set => allSubTableConvertion = value; }
        public Dictionary<string, string> TableFromAPI { get => tableFromAPI; set => tableFromAPI = value; }
        public Dictionary<string, object> AllSyncTime { get => allSyncTime; set => allSyncTime = value; }
        public Dictionary<string, object> AllPeriodTime { get => allPeriodTime; set => allPeriodTime = value; }
        public Dictionary<string, object> LeaderBoards { get => leaderBoards; set => leaderBoards = value; }
        public Dictionary<string, string> PointType { get => pointType; set => pointType = value; }
        public Dictionary<string, bool> HeaderLeaderboard { get => headerLeaderboard; set => headerLeaderboard = value; }
        public string LastTabLeaderBoard { get => lastTabLeaderBoard; set => lastTabLeaderBoard = value; }
        public Dictionary<string, string> PlaybuxNewsDict { get => playbuxNewsDict; set => playbuxNewsDict = value; }

        public Action<JObject> OnDataComplete;

        public Action<string, int> OnDataChange;



        public Action OnHeaderChange;

        public int DataChangeTarget;

        public static FirebaseAuthenticationService GetInstance()
        {

            if (instance == null)
            {
                instance = new FirebaseAuthenticationService();
            }

            return instance;

        }


        private FirebaseAuthenticationService()
        {

            userId = SystemInfo.deviceUniqueIdentifier;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {

                if (task.IsCompleted)
                {

#if !PRODUCTION
                    environment = "staging";
#endif


#if PRODUCTION
                    environment = "production";
#endif



#if DEVELOPMENT_API
            reference.Child("dev").Child("client_setting").Child("report_caching_now").ValueChanged += ErrorAPIReport;
            reference.Child("dev").Child("client_setting").Child("debug_mode").ValueChanged += DebugReport;
#endif


#if SERVER
            reference.Child(environment).Child("client_setting").Child("report_caching_now").ValueChanged += ErrorAPIReport;
            reference.Child(environment).Child("client_setting").Child("debug_mode").ValueChanged += DebugReport;
            reference.Child(environment).Child("client_setting").Child("kick_to_win_multiply_score").ValueChanged += KickToWinScoreMulitypy;
#else

                    reference.Child(environment).Child("sub_table_list").ValueChanged += SubTableList;
                    reference.Child(environment).Child("table_api").ValueChanged += TableAPI;
                    reference.Child(environment).Child("ingameui").Child("leaderboard").ValueChanged += ShowHeaderList;
#endif
                    //FetchData().Forget();
                    //PostData().Forget();
                }
                else
                {
                    Debug.LogError("FireBase:RefError");
                }
            });
        }

        public void SubWelcomeBoard(WelcomeBoard welcomeBoard)
        {
            this.welcomeBoard = welcomeBoard;
            reference.Child(environment).Child("client_setting").Child("welcomeboard").ValueChanged += WelcomeBoardData;
        }

        public void SubPlaybuxNews(PlaybuxNews playbuxNews)
        {
            this.playbuxNews = playbuxNews;
            reference.Child(environment).Child("client_setting").Child("playbuxnews").ValueChanged += PlaybuxNewsData;
        }

        public async UniTask FetchToken()
        {
            Firebase.Auth.AuthResult result = null;

            await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync("piyapong@playbux.co", "Test123!").ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    //Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    //Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                result = task.Result;
                // Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);

            });


            token = await result.User.TokenAsync(true);
            Debug.Log("[FailAPI] : " + token);

        }

        async UniTask FetchData()
        {

            await UniTask.WaitUntil(() => token != "");

            url += "?auth=" + token;
            Debug.Log(url);
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to fetch data: " + www.error);
                }
                else
                {
                    string json = www.downloadHandler.text;
                    //Debug.Log("Fetched JSON data: " + json);
                    JObject data = JsonConvert.DeserializeObject<JObject>(json);
                    ClientSettingData = data;
                    OnDataComplete?.Invoke(data);
                    Debug.Log("setting :" + json);

                }
            }
        }

        async UniTask PostData()
        {
            await UniTask.WaitUntil(() => token != "");

            urlPut += "?auth=" + token;
            Debug.Log("putTest:" + urlPut);
            string data = "{\"date\" : \"" + DateTime.UtcNow.ToString() + "\" }";
            using (UnityWebRequest www = UnityWebRequest.Put(urlPut, data))
            {
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to fetch data: " + www.error);
                }
                else
                {
                    string json = www.downloadHandler.text;

                    Debug.Log("putTestSetting :" + json);

                }
            }
        }

        void ErrorAPIReport(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("[FailAPI] :" + args.Snapshot.Value);
            //Debug.Log("isError");
            //Debug.Log(args.Snapshot.Value);
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            if (args.Snapshot.Value.ToString() == "True")
            {
                Debug.Log("[FailAPI] : isError2.0");
                FetchToken().ContinueWith(() =>
                {

                    //string fullPath = Path.Combine(Application.persistentDataPath, "APIFailStoreData.json");
                    //string recordFailedData = File.ReadAllText(fullPath);
                    string recordFailedData = PlayerPrefs.GetString("ErrorAPI");
                    PutFailAPIData(JsonConvert.DeserializeObject<JObject>(recordFailedData)).Forget();

                });

            }

        }

        void DebugReport(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("[Debug] :" + args.Snapshot.Value);

            Debug.unityLogger.logEnabled = false;

            if (args.Snapshot.Value.ToString() == "True")
            {
                Debug.Log("[Debug] : True");

                Debug.unityLogger.logEnabled = true;
            }
        }


        private void ShowHeaderList(object sender, ValueChangedEventArgs args)
        {
            foreach (var leaderBoardHeader in args.Snapshot.Children)
            {

                //Debug.Log("leaderBoardHeader.Key " + leaderBoardHeader.Key.ToString());
                //Debug.Log("leaderBoardHeader.Value " + (bool)leaderBoardHeader.Value);
                HeaderLeaderboard[leaderBoardHeader.Key] = (bool)leaderBoardHeader.Value;
            }
            OnHeaderChange?.Invoke();

        }
        void KickToWinScoreMulitypy(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("[kick_to_win_multiply_score] : " + args.Snapshot.Value);

            kickToWinMultiplyScore = int.Parse(args.Snapshot.Value.ToString());

        }

        void TableAPI(object sender, ValueChangedEventArgs args)
        {
            //Debug.Log("[TableAPI] : " + args.Snapshot.Value);

            foreach (var item in args.Snapshot.Children)
            {
                //Debug.Log(item.Value);
                TableFromAPI[item.Key] = item.Value.ToString();
            }


        }


        private void LeaderBoardList(object sender, ValueChangedEventArgs args)
        {
            //Debug.Log("LeaderBoardList key " + args.Snapshot.Key.ToString());
            // Debug.Log("LeaderBoardList value" + args.Snapshot.Value.ToString());
            foreach (var leaderBoard in args.Snapshot.Children)
            {
                var spriter = leaderBoard.Value.ToString().Split(",");
                var pointType = spriter[1];
                (object Key, object Value) item = (leaderBoard.Key, leaderBoard.Value);
                item.Value = spriter[0];
                LeaderBoards[item.Key.ToString()] = item.Value;
                PointType[spriter[0]] = pointType;
                //Debug.Log(item.Value);
                foreach (var subTable in subTableList)
                {
                    //Debug.Log(item.Value.ToString() + "." + subTable);
                    //Debug.Log("sub under");
                    reference.Child(environment).Child(item.Value.ToString()).Child(subTable).ValueChanged += PeriodTable;
                    reference.Child(environment).Child("period_time").Child(item.Value.ToString()).Child(subTable).ValueChanged += PeriodTimeScedule;
                    reference.Child(environment).Child("sync_time").Child(item.Value.ToString()).Child(subTable).ValueChanged += SyncTimeScedule;
                }

            }
            LeaderboardsCountFinish = true;

        }

        async UniTaskVoid DelaySubTime(DataSnapshot item, string subTable)
        {

            reference.Child(environment).Child(item.Value.ToString()).Child(subTable).ValueChanged += PeriodTable;
            await UniTask.Delay(1000);
            reference.Child(environment).Child("period_time").Child(item.Value.ToString()).Child(subTable).ValueChanged += PeriodTimeScedule;
            await UniTask.Delay(1000);
            reference.Child(environment).Child("sync_time").Child(item.Value.ToString()).Child(subTable).ValueChanged += SyncTimeScedule;
        }

        private void SubTableList(object sender, ValueChangedEventArgs args)
        {
            //Debug.Log("SubTableList key " + args.Snapshot.Key.ToString());
            // Debug.Log("SubTableList value" + args.Snapshot.Value.ToString());
            foreach (var item in args.Snapshot.Children)
            {
                //Debug.Log(item.Value);
                var convertion = item.Value.ToString().Split(",");
                string symbol = convertion[0];
                string display = convertion[1];
                subTableList.Add(symbol);
                allSubTableConvertion[symbol] = display;
            }

            reference.Child(environment).Child("leaderboard_list").ValueChanged += LeaderBoardList;


        }

        void PeriodTimeScedule(object sender, ValueChangedEventArgs args)
        {
            try
            {
                var allPath = args.Snapshot.Reference.Parent.ToString().Split("/");
                //Debug.Log("PeriodTimeScedule " + allPath[^1] + "." + args.Snapshot.Key.ToString());
                AllPeriodTime[allPath[^1] + "." + args.Snapshot.Key.ToString()] = args.Snapshot.Value;
                //Debug.Log(JsonConvert.SerializeObject(args.Snapshot.Value));
            }
            catch
            {

            }
        }

        void SyncTimeScedule(object sender, ValueChangedEventArgs args)
        {
            try
            {
                var allPath = args.Snapshot.Reference.Parent.ToString().Split("/");
                // Debug.Log("SyncTime " + allPath[^1] + "." + args.Snapshot.Key.ToString());
                AllSyncTime[allPath[^1] + "." + args.Snapshot.Key.ToString()] = args.Snapshot.Value;
                // Debug.Log(JsonConvert.SerializeObject(args.Snapshot.Value));
            }
            catch
            {

            }
        }

        void PeriodTable(object sender, ValueChangedEventArgs args)
        {
            try
            {
                var allPath = args.Snapshot.Reference.Parent.ToString().Split("/");
                // Debug.Log("PeriodTable " + allPath[^1] + "." + args.Snapshot.Key.ToString());
                if (!AllSubTable.ContainsKey(allPath[^1] + "." + args.Snapshot.Key.ToString()))
                {
                    if (args.Snapshot.Value == null)
                    {
                        AllSubTable[allPath[^1] + "." + args.Snapshot.Key.ToString()] = "No Table";
                        return;
                    }
                    AllSubTable[allPath[^1] + "." + args.Snapshot.Key.ToString()] = args.Snapshot.Value;
                }
                else
                {
                    if (args.Snapshot.Value == null)
                    {
                        return;
                    }
                    AllSubTable[allPath[^1] + "." + args.Snapshot.Key.ToString()] = args.Snapshot.Value;
                    OnDataChange?.Invoke(lastTabLeaderBoard, -1);
                }
            }
            catch
            {

            }
        }

        void WelcomeBoardData(object sender, ValueChangedEventArgs args)
        {
            //Debug.Log("[WelcomeBoard] : " + args.Snapshot.Value);
#if FIREBASE_BYPASS
            return;
#endif
            WelcomeBoardImage = args.Snapshot.Value.ToString();
            //Debug.Log("[WelcomeBoard] : " + WelcomeBoardImage);
            welcomeBoard.ChangeImage(WelcomeBoardImage);
        }

        void PlaybuxNewsData(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("[PlaybuxNews] : " + args.Snapshot.Value);
#if FIREBASE_BYPASS
            return;
#endif
            foreach (var item in args.Snapshot.Children)
            {
                string key = item.Key.ToString();
                string data = item.Value.ToString();
                Debug.Log(key +  " : " + data );
                PlaybuxNewsDict[key] = data;
            }

            playbuxNews.SetNewsData(PlaybuxNewsDict);
        }

        public async UniTask PutFailAPIData(JToken data)
        {
            urlAPIFailData += "?auth=" + token;
            var failData = data;
            //var failData = new JObject();
            //failData["data"] = data;
            //failData["hash"] = failData["data"] + "playbux";
            //Debug.Log("[FailAPI] : " + JsonConvert.SerializeObject(failData));

            using (UnityWebRequest www = UnityWebRequest.Put(urlAPIFailData, JsonConvert.SerializeObject(failData)))
            {

                await www.SendWebRequest();


                if (www.result != UnityWebRequest.Result.Success)
                {

                    if (www.error.Contains("40"))
                    {
                        //Debug.Log("[FailAPI] : API Fail");
                    }
#if DEVELOPMENT
                    Debug.LogError("Failed to fetch data: " + www.error);
#endif

                }
                else
                {
                    string json = www.downloadHandler.text;

                    Debug.Log("[FailAPI] :" + json);

                }
            }
        }

    }

}
