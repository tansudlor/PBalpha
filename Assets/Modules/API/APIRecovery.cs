using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using com.playbux.firebaseservice;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace com.playbux.api
{

    public class RecordFailed
    {
        List<FailAPI> failAPIs;
        public List<FailAPI> FailAPIs { get => failAPIs; set => failAPIs = value; }
    }

    public class APIRecovery
    {

        private RecordFailed recordFailed;

        private static APIRecovery instance;

        private string recoveryFile = Path.Combine(Application.persistentDataPath, "APIFailStoreData.json");
        public static APIRecovery GetInstante()
        {
            if (instance == null)
            {
                instance = new APIRecovery();
            }
            return instance;
        }


        public void LoadRecovery()
        {

            /*this.recoveryFile = recoveryFile;
            if (recoveryFile == null)
            {
                this.recoveryFile = Path.Combine(Application.persistentDataPath, "APIFailStoreData.json");
            }*/

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("ErrorAPI")) )
            {
               // Debug.Log("ErrorAPI");
                //string recoveryFileData = File.ReadAllText(recoveryFile);
                string recoveryFileData = PlayerPrefs.GetString("ErrorAPI");
                recordFailed = JsonConvert.DeserializeObject<RecordFailed>(recoveryFileData);
            }
            else   
            {
               // Debug.Log("ErrorAPI1");
                recordFailed = new RecordFailed();
                recordFailed.FailAPIs = new List<FailAPI>();
                //File.WriteAllText(this.recoveryFile, JsonConvert.SerializeObject(recordFailed));
                PlayerPrefs.SetString("ErrorAPI", JsonConvert.SerializeObject(recordFailed));
            }
        }

        public void ReportAPIFailData(string nameAPI, string request, string response, bool complete)
        {
            //Debug.Log(nameAPI);
            //Debug.Log(request);
            //Debug.Log(response);
           // Debug.Log(complete);

            FailAPI failAPI = new FailAPI();
            failAPI.NameAPI = nameAPI;
            failAPI.Request = request;
            failAPI.Response = response;
            failAPI.Complete = complete;
            failAPI.DateTime = DateTime.UtcNow.ToString("s");
            failAPI.RetryCount = 1;
            recordFailed.FailAPIs.Add(failAPI);
        }

        public void Save()
        {
            string recordFailedData = JsonConvert.SerializeObject(recordFailed);
            if (string.IsNullOrEmpty(recordFailedData))
            {
                return;
            }

            PlayerPrefs.SetString("ErrorAPI", recordFailedData);
            //File.WriteAllText(recoveryFile, recordFailedData);
        }


        public void SaveToFirebase()
        {
            string recordFailedData = JsonConvert.SerializeObject(recordFailed);
            if (string.IsNullOrEmpty(recordFailedData))
            {
                return;
            }

            FirebaseAuthenticationService.GetInstance().PutFailAPIData(JsonConvert.DeserializeObject<JObject>(recordFailedData)).Forget();
        }

        public async void ResendRequest()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("ErrorAPI")))
            {
                return;
            }

            if(recordFailed.FailAPIs == null)
            { 
                return;
            }

            if (recordFailed.FailAPIs.Count <= 0)
            {
                return;
            }

            if (recordFailed.FailAPIs[0] == null)
            {
                return;
            }

           
            Debug.Log("recordFailed.FailAPIs.Count" + recordFailed.FailAPIs.Count);
            Debug.Log("recordFailed.FailAPIs.Count" + JsonConvert.SerializeObject(recordFailed.FailAPIs[0]));

            bool isSend = false;
            string request = "";
            string response = "";
            string path = "";
            int dailyquestErrorCount = 0;
            for (int i = recordFailed.FailAPIs.Count - 1; i >= 0; i--)
            {

                if (recordFailed.FailAPIs[i].NameAPI == APIServerConnector.ApiGame + "/operations/quiz-question/random")
                {
                    if (dailyquestErrorCount > 0)
                    {
                        if (!recordFailed.FailAPIs[i].Complete)
                        {
                            recordFailed.FailAPIs[i].Complete = true;
                        }
                    }
                    dailyquestErrorCount++;
                }

                if (recordFailed.FailAPIs[i].Complete)
                {
                
                    recordFailed.FailAPIs.RemoveAt(i);
                    Debug.Log("recordFailed.FailAPIs.Count after remove" + recordFailed.FailAPIs.Count);
                }
            }


            if (recordFailed.FailAPIs.Count <= 0)
            {
                Save();
                return;
            }

            //Debug.Log("where Request " + recordFailed.FailAPIs[0].Request);

            //Debug.Log("where Response " + recordFailed.FailAPIs[0].Response);
            try
            {

                (isSend, response, request, path) = await APIServerConnector.APIPostRecovery(recordFailed.FailAPIs[0].NameAPI, recordFailed.FailAPIs[0].Request);
                recordFailed.FailAPIs[0].Request = request;
                recordFailed.FailAPIs[0].Response = response;
                recordFailed.FailAPIs[0].Complete = isSend;
                recordFailed.FailAPIs[0].DateTime = DateTime.UtcNow.ToString("s");
                recordFailed.FailAPIs[0].RetryCount = recordFailed.FailAPIs[0].RetryCount + 1;
                if (!recordFailed.FailAPIs[0].Complete)
                {
                    recordFailed.FailAPIs[0].Complete = true;
                    FailAPI pushBack = new FailAPI();
                    pushBack.Request = request;
                    pushBack.Response = response;
                    pushBack.Complete = isSend;
                    pushBack.DateTime = DateTime.UtcNow.ToString("s");
                    pushBack.RetryCount = recordFailed.FailAPIs[0].RetryCount + 1;
                    pushBack.Comment = "Recovery Failed";
                    recordFailed.FailAPIs.Add(pushBack);
                }
            }
            catch
            {
                recordFailed.FailAPIs[0].Complete = true;
                FailAPI pushBack = new FailAPI();
                pushBack.Request = recordFailed.FailAPIs[0].Request;
                pushBack.Response = recordFailed.FailAPIs[0].Response;
                pushBack.Complete = false;
                pushBack.DateTime = DateTime.UtcNow.ToString("s");
                pushBack.RetryCount = recordFailed.FailAPIs[0].RetryCount + 1;
                pushBack.Comment = "Recovery crash!";
                recordFailed.FailAPIs.Add(pushBack);


            }
            Save();
        }

    }

}
