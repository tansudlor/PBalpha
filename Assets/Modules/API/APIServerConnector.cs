using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using Firebase.Database;
using com.playbux.firebaseservice;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEditor;

namespace com.playbux.api
{
    public class APIServerConnector
    {
        public static Dictionary<string, UserProfile> userProfileCache = new Dictionary<string, UserProfile>();
        public static Dictionary<string, FlagRewardCostData> flagRewardCostData = new Dictionary<string, FlagRewardCostData>();

#if DEVELOPMENT_API
        private static string apiGame = "https://playbux-game-backend-api.insitemedia.co.th/v1";
        public static string logInURL = "https://playbux-account.insitemedia.co.th/login?serviceUid=0847cbf0-1c39-40b0-8de0-98b2987cba63";
        public static string serviceUid = "0847cbf0-1c39-40b0-8de0-98b2987cba63";
        public static string apiKey = "playbux";
        private static string apiLogin = "https://playbux-account-api.insitemedia.co.th/v1";
        

#endif

#if !PRODUCTION //client server staging
        private static string apiGame = "https://playbux-game-backend-api.insitemedia.co.th/v1";
        public static string logInURL = "https://playbux-account.insitemedia.co.th/login?serviceUid=0847cbf0-1c39-40b0-8de0-98b2987cba63";
        public static string serviceUid = "0847cbf0-1c39-40b0-8de0-98b2987cba63";
        public static string apiKey = "playbux";
        private static string apiLogin = "https://playbux-account-api.insitemedia.co.th/v1";

#endif


#if !SERVER && PRODUCTION //client production
        private static string apiGame = "https://game-api.playbux.co/v1";
        public static string serviceUid = "cdb8ffff-eea1-43d7-979a-9cb9fdec04ee";
        public static string logInURL = "https://account.playbux.co/login?serviceUid=" + serviceUid;
        public static string apiKey = "";
        private static string apiLogin = "https://account-api.playbux.co/v1";
        public static string gameClientVersion = "1.0.0.0";
        
#endif


#if SERVER && PRODUCTION //server production
        private static string apiGame = "https://game-api.playbux.co/v1";
        public static string serviceUid = "cdb8ffff-eea1-43d7-979a-9cb9fdec04ee";
        public static string logInURL = "https://account.playbux.co/login?serviceUid=" + serviceUid;
        public static string apiKey = "E16XV5F_RJ41ROR6ivl2T";
        private static string apiLogin = "https://account-api.playbux.co/v1";

#endif
        public static string ApiGame { get => apiGame; set => apiGame = value; }
        public static void SetAPIDynamicPath()
        {
#if DEVELOPMENT_API
            FirebaseDatabase.DefaultInstance.RootReference.Child("dev").Child("api_path").ValueChanged += (object sender, ValueChangedEventArgs args) =>
            {
                apiGame = args.Snapshot.Value.ToString();
            };     
#endif
        }

        public static long SecondToTicks(int seconds)
        {
            return seconds * 10000000;
        }

        public static int TicksToSecond(long ticks)
        {
            return (int)(ticks / 10000000);
        }


        public static async UniTask<UserProfile> GetMe(string accessToken)
        {
#if API_BYPASS

            UserProfile userProfile = new UserProfile();
            userProfile.uid = System.Guid.NewGuid().ToString();
            userProfile.equipments = new Equipments();
            userProfile.updatedAt = DateTime.Now;
            userProfile.createdAt = DateTime.Now;
            userProfile.display_name = "John" + Environment.TickCount;
            userProfile.user_name = userProfile.display_name;
            userProfile._id = System.Guid.NewGuid().ToString();
            userProfile.balance_brk = 12;
            userProfile.balance_lotto_tickets = 12;
            userProfile.accressToken = accessToken;
            userProfileCache[userProfile.uid] = userProfile;
            return userProfile;

#endif

            string apiUrl = ApiGame + "/me";
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);


                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    return null;
                }


                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {

                    UserProfile profile = JsonConvert.DeserializeObject<UserProfile>(request.downloadHandler.text);
                    profile.accressToken = accessToken;
                    bool canPlayQuiz = await GetMeMetaData(accessToken);
                    profile.canPlayQuiz = canPlayQuiz;
                    userProfileCache[profile.uid] = profile;
                    return profile;
                }
            }

        }

        public static async UniTask<bool> GetMeMetaData(string accessToken)
        {
            string apiUrl = ApiGame + "/me/metadata";
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);


                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    return false;
                }


                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return false;
                }
                else
                {

                    JObject metaData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    bool canPlayQuiz = metaData["can_play_quiz"].ToObject<bool>();
                    return canPlayQuiz;
                }
            }
        }

        public static async UniTask<string> UpdateAvatar(string userToken, string bodyData)
        {

#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/me/equipments";
            using (UnityWebRequest request = UnityWebRequest.Put(apiUrl, bodyData))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + userToken);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return request.result.ToString();
                }
                else
                {

                    return request.downloadHandler.text;
                }
            }

        }

        public static async UniTask<string> UpdateName(string userToken, string bodyData)
        {
#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/me/display_name";
            using (UnityWebRequest request = UnityWebRequest.Put(apiUrl, bodyData))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + userToken);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return request.result.ToString();
                }
                else
                {

                    return request.downloadHandler.text;
                }
            }
        }

        public static async UniTask<List<UserFlag>> GetMeFlag(string accessToken, int deep = 0)
        {
#if API_BYPASS
            return new List<UserFlag>();
#endif
            string apiUrl = ApiGame + "/me/flags";

            if (deep > 2)
            {
                //FIXME: If cannot get flag For server kick player off Server

                return null;

            }

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {

                    return await GetMeFlag(accessToken, deep + 1);
                }


                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return await GetMeFlag(accessToken, deep + 1);
                }
                else
                {
                    List<UserFlag> flagList = JsonConvert.DeserializeObject<List<UserFlag>>(request.downloadHandler.text);
                    return flagList;
                }
            }
        }

        public static async UniTask<JObject> QuestList(string accessToken)
        {
#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/me/quests?limit=0&skip=0";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return null;
                }
                else
                {
                    JObject quests = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);

                    return quests;
                }

            }

        }

        public static async UniTask<JObject> InventoryAPI(string accessToken)
        {

            string apiUrl = ApiGame + "/me/inventory/query?limit=0";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("accept", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return null;
                }
                else
                {
                    JObject data = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);

                    return data;
                }

            }

        }


        public static async UniTask<QuestDetails> StartQuest(string accessToken, string questId)
        {
#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/quests/" + questId;
            Debug.Log("[StartQuest] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return null;
                }
                else
                {
                    QuestDetails questDetail = JsonConvert.DeserializeObject<QuestDetails>(request.downloadHandler.text);
                    Debug.Log("[StartQuest] : " + questDetail);
                    return questDetail;
                }

            }
        }

        public static async UniTask<SyncResponse> SyncAPI(string accessToken, int deep = 0)
        {
#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/me/sync";
            if (deep > 2)
            {
                SyncResponse syncResponse = new SyncResponse();
                syncResponse.success = false;
                return syncResponse;
            }


            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, "{}", "application/json"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {

                    return await SyncAPI(accessToken, deep + 1);
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {

                    return await SyncAPI(accessToken, deep + 1);
                }
                else
                {
                    SyncResponse syncResponse = JsonConvert.DeserializeObject<SyncResponse>(request.downloadHandler.text);
                    return syncResponse;
                }

            }

        }

        public static async UniTask<string> GameFlagAPI()
        {
#if API_BYPASS
            return null;
#endif
            string apiUrl = ApiGame + "/flags";
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return "false";
                }
                else
                {
                    List<FlagRewardCostData> flagList = JsonConvert.DeserializeObject<List<FlagRewardCostData>>(request.downloadHandler.text);
                    foreach (var flagData in flagList)
                    {

                        flagRewardCostData[flagData.Name] = flagData;

                    }
                    return "true";
                }
            }
        }

        public static async UniTask<FlagAddRemoveResponse> AddUserFlag(string clientID, string flagName, string accessToken)
        {
#if API_BYPASS
            return null;
#endif

            string apiUrl = ApiGame + "/operations/users/" + clientID + "/flags/" + flagName;
            Debug.Log("[AddUserFlag] : " + clientID + " : " + flagName + " : " + accessToken);
            Debug.Log("[AddUserFlag] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, "{}", "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[AddUserFlag] : " + request.error);
                    return null;
                }
                else
                {
                    FlagAddRemoveResponse flagResponse = JsonConvert.DeserializeObject<FlagAddRemoveResponse>(request.downloadHandler.text);

                    Debug.Log("[AddUserFlag] : " + request.downloadHandler.text);
                    return flagResponse;
                }
            }
        }

        public static async UniTask<string> SendUserList(string json)
        {

            string apiUrl = ApiGame + "/operations/online-user-stats";
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, json, "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);


                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {

                    return request.downloadHandler.text;
                }
            }
        }


        public class DevSchedul
        {
            public List<string> schedule_times { get; set; }
        }

        public static async UniTask<string> SchedulesAPI(string apiRoute, int timeWait = 5)
        {
            string apiUrl = ApiGame + apiRoute;


#if UNITY_EDITOR

            apiUrl = apiGame + "/operations/quiz-events/663d9a96bc965c3e7f4c743c/quiz-sessions";
            
            DevSchedul devSchedul = new DevSchedul();
            devSchedul.schedule_times = new List<string>();
            for (int i = 0; i < 24 * 60; i += timeWait)
            {
                var time = String.Format("{0}:{1}", (i / 60).ToString("00"), (i % 60).ToString("00"));
            
                Debug.Log(time);
                devSchedul.schedule_times.Add(time);
            }
            
            
            //Debug.Log(JsonConvert.SerializeObject(devSchedul));
            
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, JsonConvert.SerializeObject(devSchedul), "application/json"))
            {
            
                request.SetRequestHeader("x-api-key", apiKey);
            
            
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {
                    //SchedulesData schedulesData = JsonConvert.DeserializeObject<SchedulesData>(request.downloadHandler.text);
                    Debug.Log(request.downloadHandler.text);
                    //Debug.Log(schedulesData);
                    return "{ \"data\":" + request.downloadHandler.text + "}";
                }
            }


#endif


            var dateISO8601 = DateTime.UtcNow.ToString("s");

            WWWForm form = new WWWForm();
            form.AddField("date", dateISO8601);

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);


                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[schedulesData] : " + request.error);
                    return null;
                }
                else
                {
                    //SchedulesData schedulesData = JsonConvert.DeserializeObject<SchedulesData>(request.downloadHandler.text);
                    Debug.Log("[schedulesData] : " + request.downloadHandler.text);
                    //Debug.Log(schedulesData);
                    return request.downloadHandler.text;
                }
            }

        }

        public static async UniTask<string> GetQuizEvent(string quizId)
        {
            string apiUrl = ApiGame + "/operations/quiz/" + quizId + "?show_answer=true&shuffle_question=true&shuffle_choice=true&total_question=5";
            Debug.Log("[GetQuizEvent] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {

                request.SetRequestHeader("x-api-key", apiKey);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[GetQuizEvent] : " + request.error);
                    return null;
                }
                else
                {
                    string quizData = request.downloadHandler.text;
                    Debug.Log("[GetQuizEvent] : " + apiUrl);
                    Debug.Log("[GetQuizEvent] : " + quizData);
                    return quizData;
                }
            }
        }

        public static async UniTask AnalyticsAPI(string analyticsData)
        {
            string apiUrl = ApiGame + "/analytic";
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, analyticsData, "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");

                try
                {
                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);

                    }
                    else
                    {
                        Debug.Log(request.downloadHandler.text + " AnalyticsAPI");

                    }
                }
                catch
                {

                }
            }
        }

        public static async UniTask<(bool, string, string, string)> GetDailyQuest() //1) success 2)respone 3)request 4)pathAPI
        {

            string apiUrl = ApiGame + "/operations/quiz-question/random?type=DailyQuest&total_quiz=1&total_question=1&show_answer=true&shuffle_question=false&shuffle_choice=false";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return (false, request.error, apiUrl, apiUrl);
                }
                else
                {
                    string quizData = request.downloadHandler.text;
                    JObject data = JsonConvert.DeserializeObject<JObject>(quizData);

                    try
                    {
                        data["data"][0]["questions"]["choices"][0].ToString();
                    }
                    catch (Exception ex)
                    {
                        return (false, quizData, apiUrl, apiUrl);
                    }

                    return (true, quizData, apiUrl, apiUrl);
                }
            }
        }

        public static async UniTask<(bool, string, string, string)> QuizAnswerAPI(string quizId, string questionId, List<QuizAnswer> answers)
        {
            string apiUrl = ApiGame + "/operations/quiz-answers";

            Debug.Log("[QuizAnswer] : " + apiUrl);
            Debug.Log("[QuizAnswer] : answers : " + JsonConvert.SerializeObject(answers));

            QuizAnswerRequest quizAnswerRequest = new QuizAnswerRequest();
            quizAnswerRequest.quiz_session = quizId;
            quizAnswerRequest.question = questionId;
            quizAnswerRequest.answers = answers;
            quizAnswerRequest.failed_case = false;

            string requestBody = JsonConvert.SerializeObject(quizAnswerRequest);

            Debug.Log("[QuizAnswer] : quizAnswerRequest : " + requestBody);

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, requestBody, "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);

                try
                {
                    await request.SendWebRequest();

                }
                catch (UnityWebRequestException e)
                {
                    Debug.Log(e.Message + " e QuizAnswerError");
                    return (false, e.Message, requestBody, apiUrl);
                }


                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return (false, request.error, requestBody, apiUrl);
                }
                else
                {
                    //SchedulesData schedulesData = JsonConvert.DeserializeObject<SchedulesData>(request.downloadHandler.text);
                    Debug.Log("[QuizAnswer] : quizAnswerRequestBody : " + requestBody);
                    Debug.Log("[QuizAnswer] : quizAnswerRequestDownloadHandler : " + request.downloadHandler.text);
                    //Debug.Log(schedulesData);
                    return (true, request.downloadHandler.text, requestBody, apiUrl);
                }


            }
        }

        public static async UniTask<(bool, string, string, string)> APIPostRecovery(string nameAPI, string requestBody)
        {
            string apiUrl = nameAPI;

            Debug.Log("request" + requestBody);

            JObject jsonRequest = JsonConvert.DeserializeObject<JObject>(requestBody);
            jsonRequest["failed_case"] = true;

            requestBody = JsonConvert.SerializeObject(jsonRequest);

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, requestBody, "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);

                try
                {
                    await request.SendWebRequest();

                }

                catch (UnityWebRequestException e)
                {
                    Debug.LogError(e.Message);
                    return (false, e.Message, requestBody, apiUrl);
                }


                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return (false, request.error, requestBody, apiUrl);
                }
                else
                {
                    Debug.Log(request.downloadHandler.text);

                    return (true, request.downloadHandler.text, requestBody, apiUrl);
                }


            }
        }



        public static async UniTask<JObject> QueueMe(string accessToken) //questID is number of order quest not _id
        {

            string apiUrl = ApiGame + "/me/online-queue";
            Debug.Log("[QuereMeRequest] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, "{}", "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                try
                {
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                       request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("[QuereMeRequest] : " + request.error);
                        return null;
                    }
                    else
                    {
                        JObject queueData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                        Debug.Log("[QuereMeRequest] : " + request.downloadHandler.text);
                        return queueData;
                    }

                }
                catch (Exception e)
                {
                    //Debug.Log("QueueMe eMessage " + e.Message);
                    return null;
                }
            }
        }

        public static async UniTask<string> AddQuestFlag(string uid, string flagName, string questID) //questID is number of order quest not _id
        {
#if API_BYPASS
            return null;
#endif
            if (questID == "0")
            {
                return "";
            }

            Debug.Log("[AddQuestFlag] :  " + uid);
            Debug.Log("[AddQuestFlag] : " + flagName);
            Debug.Log("[AddQuestFlag] : " + questID);

            string apiUrl = ApiGame + "/operations/quests/" + questID + "/progress";

            FlagDataSendToAPI flagDataSendToAPI = new FlagDataSendToAPI();
            flagDataSendToAPI.userId = uid;
            flagDataSendToAPI.flag = flagName;

            Debug.Log("[AddQuestFlag] : " + apiUrl);
            Debug.Log("[AddQuestFlag] : " + JsonConvert.SerializeObject(flagDataSendToAPI));

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, JsonConvert.SerializeObject(flagDataSendToAPI), "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);
                try
                {
                    await request.SendWebRequest();
                }
                catch (Exception ex)
                {
                    Debug.Log("[AddQuestFlag] : " + ex.Message);
                    Debug.Log("- from [AddQuestFlag] : " + JsonConvert.SerializeObject(flagDataSendToAPI));
                }
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                   request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[AddQuestFlag] : " + request.error);
                    Debug.Log("- from [AddQuestFlag] : " + JsonConvert.SerializeObject(flagDataSendToAPI));
                    return null;
                }
                else
                {
                    JObject addQuestData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    bool userReward = addQuestData["userRewards"] != null;
                    if (!userReward)
                    {
                        return null;
                    }
                    Debug.Log("[AddQuestFlag] : " + request.downloadHandler.text);
                    Debug.Log("- from [AddQuestFlag] : " + JsonConvert.SerializeObject(flagDataSendToAPI));
                    return "updatebalance";
                }

            }
        }


        public static async UniTask<bool> RemoveUserFlag(string clientID, string flagname, string accessToken)
        {
#if API_BYPASS
            return false;
#endif
            string apiUrl = ApiGame + "/operations/users/" + clientID + "/flags/" + flagname;
            Debug.Log("[RemoveUserFlag] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Delete(apiUrl))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[RemoveUserFlag] : " + request.error);
                    return false;
                }
                else
                {

                    Debug.Log("Success Remove Flag");
                    return true;
                }
            }
        }



        public static async UniTask<string> GetWhiteListToken(string userToken)
        {
            string apiUrl = "https://script.google.com/macros/s/AKfycbzeZxE7cUef0KIfkVKsVuaSYlghCsZdfnc8ya5ffHrJ0RRYAsGBerG80-oFpZWnncXXPQ/exec?passcode=" + userToken;
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {
                    string tokenResponse = request.downloadHandler.text;
                    return tokenResponse;
                }
            }
        }

        public static async UniTask<AuthResponse> LogIn(string email, string password)
        {
            string apiUrl = apiLogin + "/auth/user/login";

            WWWForm form = new WWWForm();
            form.AddField("serviceUid", serviceUid);
            form.AddField("email", email);
            form.AddField("password", password);



            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
            {
                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    Debug.Log(e.Message);
                    return null;
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {
                    AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);
                    Debug.Log(request.downloadHandler.text);

                    return authResponse;
                }
            }

        }

        public static async UniTask<AccessTokenResponse> RefreshTokenAPI(string refreshToken)
        {

            string apiUrl = apiLogin + "/auth/user/refresh_token";

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, "{}", "application/x-www-form-urlencoded"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + refreshToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    return null;
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {
                    AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(request.downloadHandler.text);

                    return accessTokenResponse;
                }
            }

        }

        public static async UniTask<string> LogOutAPI(string accessToken)
        {

            string apiUrl = apiLogin + "/auth/user/logout";



            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, "", "application/x-www-form-urlencoded"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    Debug.Log(e.Message);
                    return null;
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);

                    return null;
                }
                else
                {
                    JObject logout = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    var logoutMessgae = logout["message"].ToString();
                    Debug.Log(logoutMessgae);

                    return logoutMessgae;
                }
            }

        }

        public static async UniTask<QuestionGame> QuestionGameAPI()
        {
            string apiUrl = "https://script.google.com/macros/s/AKfycbwzJTDX_kAB_nEeuiQHE0oyqiEXiRmqZCcjpJQ007mzvoreSD8kJhYHE2raZR-V8GQm/exec?action=getQuest";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return null;
                }
                else
                {
                    Debug.Log(request.downloadHandler.text);
                    QuestionGame questionGame = JsonConvert.DeserializeObject<QuestionGame>(request.downloadHandler.text);
                    return questionGame;
                }
            }
        }


        public static async UniTask<bool> LeaveWaitingQueue(string accessToken)
        {
            string apiUrl = ApiGame + "/me/waiting-queue";
            Debug.Log("[LeaveWaitingQueue] : " + apiUrl);
            using (UnityWebRequest request = UnityWebRequest.Delete(apiUrl))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[LeaveWaitingQueue] : " + request.error);
                    return false;
                }
                else
                {
                    Debug.Log("[LeaveWaitingQueue] : " + request.downloadHandler.text);
                    return true;
                }
            }
        }


        public static async UniTask<(bool, JToken)> GameAPIResult(string game_key, List<GameResultAPI> game_results, string result_type, string result_operator, string guid)
        {

            string apiUrl = ApiGame + "/operations/game-results";
            Debug.Log("[GameAPIResult] : " + apiUrl);


            GameDataAPI gameDataAPI = new GameDataAPI();
            gameDataAPI.GameKey = game_key;

            //game_results
            gameDataAPI.GameResults = game_results;
            gameDataAPI.ResultType = result_type;
            gameDataAPI.ResultOperator = result_operator;

            gameDataAPI.SessionKey = guid;

            string dateTime = DateTime.UtcNow.ToString("o");
            gameDataAPI.SessionDate = dateTime;

            string jsonGameDatAPI = JsonConvert.SerializeObject(gameDataAPI);

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, jsonGameDatAPI, "application/json"))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("x-api-key", apiKey);
                try
                {
                    await request.SendWebRequest();
                }
                catch (Exception ex)
                {
                    Debug.Log("[GameAPIResult] : " + ex.Message);
                    Debug.Log("- from [GameAPIResult] : " + jsonGameDatAPI);
                    APIRecovery.GetInstante().ReportAPIFailData(apiUrl, jsonGameDatAPI, ex.Message, false);
                    APIRecovery.GetInstante().Save();
                }
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[GameAPIResult] : " + request.error);
                    Debug.Log("- from [GameAPIResult] : " + jsonGameDatAPI);
                    APIRecovery.GetInstante().ReportAPIFailData(apiUrl, jsonGameDatAPI, request.error, false);
                    APIRecovery.GetInstante().Save();
                    return (false, null);
                }
                else
                {
                    Debug.Log("[GameAPIResult] : " + request.downloadHandler.text);
                    Debug.Log("- from [GameAPIResult] : " + jsonGameDatAPI);
                    JToken reward = null;
                    try
                    {
                        JObject jData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                        reward = jData["userTxs"];

                    }
                    catch
                    {
                        reward = null;
                    }
                    return (true, reward);

                }

            }


        }


        public static async UniTask<(JObject, string)> MeRanking(string gameKey, string rankingType, string accessToken)
        {
            string apiUrl = ApiGame + "/me/ranking/" + gameKey + "/" + rankingType;

            Debug.Log("MeRanking " +apiUrl);

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("accept", "*/*");
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    return (null, null);
                }
                else
                {
                    string RankingData = request.downloadHandler.text;
                    JObject data = JsonConvert.DeserializeObject<JObject>(RankingData);
                    return (data, apiUrl);
                }
            }

        }

    }

    public class FlagDataSendToAPI
    {
        public string userId { get; set; }
        public string flag { get; set; }
    }

    public class QuizAnswerRequest
    {
        public string quiz_session { get; set; }
        public string question { get; set; }
        public List<QuizAnswer> answers { get; set; }

        public bool failed_case { get; set; }
    }

    public class UserProfile //User Data from API
    {
        [JsonProperty("uid")]
        public string _id { get; set; }
        [JsonProperty("_id")]
        public string uid { get; set; }
        public string user_name { get; set; }
        public string display_name { get; set; }
        public Equipments equipments { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int balance_brk { get; set; }
        public int balance_lotto_tickets { get; set; }
        public string accressToken { get; set; }
        public Wallet wallet { get; set; }

        public string email { get; set; }

        public bool canPlayQuiz { get; set; }


    }


    public class Wallet
    {
        [JsonProperty("PEBBLE")]
        public Currency pebble { get; set; }
        [JsonProperty("BRK")]
        public Currency brk { get; set; }
        [JsonProperty("PBUX")]
        public Currency pbux { get; set; }
        [JsonProperty("LOTTO_TICKET")]
        public Currency lotto_ticket { get; set; }
        [JsonProperty("LOTTO_PIECE")]
        public Currency lotto_piece { get; set; }
        [JsonProperty("WEEKLY_POINT")]
        public Currency weekly_point { get; set; }

    }

    public class Currency
    {
        public int amount_unsafe { get; set; }
        public string amount_ether { get; set; }
        public int locked_unsafe { get; set; }
        public string locked_ether { get; set; }
        public int reserved_unsafe { get; set; }
        public string reserved_ether { get; set; }
    }


    public class AuthResponse
    {
        public string accessToken;
        public string refreshToken;
        public int userId;
        public string email;
        public string name;
        public List<string> roles;

    }
    public class AccessTokenResponse
    {
        public string accessToken;
    }

    public class SyncResponse
    {
        public bool success;
    }

    public class UserFlag
    {
        [JsonProperty("_id")]
        public string FlagId { get; set; }
        [JsonProperty("user")]
        public string UID { get; set; }
        public string status { get; set; }
        public FlagList flag { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class FlagList
    {
        public string status { get; set; }
        public string type { get; set; }
        public List<string> tags { get; set; }
        public string _id { get; set; }
        public string name { get; set; }
        public bool reward { get; set; }
        public List<Reward> rewards { get; set; }
        public int version { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public List<Cost> costs { get; set; }
    }


    public class Reward
    {
        public string type { get; set; }
        public int quantity { get; set; }
    }

    public class Cost
    {
        public string type { get; set; }
        public int quantity { get; set; }
    }


    public class FlagAddRemoveResponse
    {
        [JsonProperty("user")]
        public string UID { get; set; }
        public string status { get; set; }
        public string flag { get; set; }
        [JsonProperty("_id")]
        public string FlagId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class QuestDetails
    {
        public string _id { get; set; }
        public DateTime createdAt { get; set; }
        public bool isActive { get; set; }
        public string[] steps { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public DateTime updatedAt { get; set; }
        public int version { get; set; }
    }

    public class FlagRewardCostData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Reward { get; set; }
        public List<Reward> Rewards { get; set; }
        public List<Cost> Costs { get; set; }

    }

    public class UserBalance
    {
        public int Brk { get; set; }
        public int Lotto { get; set; }
        public int Pbux { get; set; }

        public int Pebble { get; set; }
        public int LottoPiece { get; set; }
    }

    public class QuestionGame
    {
        public string eventname { get; set; }
        public List<Question> questiongame { get; set; }

    }
    public class Question
    {
        public string questionId { get; set; }
        public string question { get; set; }
        public Dictionary<int, JToken> choices { get; set; }
        public int correctAnswer { get; set; }
        public string rewards { get; set; }
    }

    public class QuestionAPI
    {
        public string Id { get; set; }
        public int AnswerNo { get; set; }
        public List<Choice> Choices { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
    }

    public class Choice
    {
        public int No { get; set; }
        public string Description { get; set; }
        public bool IsCorrect { get; set; }
        public string Id { get; set; }
    }

    public class QuizAnswer
    {
        public string user { get; set; }
        public int answer { get; set; }
    }

    public class OnlineUser
    {
        List<UserDataToAPI> users;

        [JsonProperty("users")]
        public List<UserDataToAPI> Users { get => users; set => users = value; }
    }

    public class UserDataToAPI
    {
        public string user_id { get; set; }
        public bool is_vip { get; set; }
        public int total_step { get; set; }
        public int total_distance { get; set; }
        public int online_duration { get; set; }

    }

    public class FailAPI
    {
        public string NameAPI { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public bool Complete { get; set; }
        public string DateTime { get; set; }
        public int RetryCount { get; set; }
        public string Comment { get; set; }
    }

    class RestAPIAnalyticsData
    {
        private string eventName;
        private LogParameter[] parameters;
        [JsonProperty("event_name")]
        public string EventName { get => eventName; set => eventName = value; }
        [JsonProperty("parameters")]
        public LogParameter[] Parameters { get => parameters; set => parameters = value; }
        public RestAPIAnalyticsData(string eventName, LogParameter[] parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }


    }

    class LogParameter
    {
        private string key;
        private string value;
        [JsonProperty("key")]
        public string Key { get => key; set => key = value; }
        [JsonProperty("value")]
        public string Value { get => value; set => this.value = value; }
        public LogParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }


    }

    public class GameDataAPI
    {
        [JsonProperty("game_key")]
        public string GameKey { get; set; }

        [JsonProperty("game_results")]
        public List<GameResultAPI> GameResults { get; set; }

        [JsonProperty("result_type")]
        public string ResultType { get; set; }

        [JsonProperty("result_operator")]
        public string ResultOperator { get; set; }

        [JsonProperty("session_key")]
        public string SessionKey { get; set; }
        [JsonProperty("session_date")]
        public string SessionDate { get; set; }
    }

    public class GameResultAPI
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("results")]
        public List<ResultAPI> Results { get; set; }
    }

    public class ResultAPI
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

}
