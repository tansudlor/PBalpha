#if SERVER
using com.playbux.flag;
using com.playbux.networking.mirror.message;
using com.playbux.npc;
using com.playbux.tool;
using Mirror;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using com.playbux.quest;
using com.playbux.identity;
using com.playbux.api;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

namespace com.playbux.networkquest
{

    public sealed partial class QuestRunner : IQuestRunner
    {
        private IIdentitySystem identitySystem;
        private NPCModel model;
        private IFlagCollection<string> flags;
        private IServerNetworkMessageReceiver<QuestMessage> messageReceiver;
        private bool updateDailyQuestNow = false;
        private string dailyQuestData = null;
        private string correctNodeId = "63d9c779-a251-4455-ba9b-826a0ebf1279638499829872787033";
        private string wrongNodeId = "8cc62dbb-65ea-4bf3-aec2-e6c84031d0d3638471472143254483";
        public QuestRunner(IServerNetworkMessageReceiver<QuestMessage> messageReceiver, NPCModel model, IFlagCollection<string> flags, IIdentitySystem identitySystem)
        {
#if DEVELOPMENT
            Debug.Log("QRM sub");
#endif
            this.model = model;
            this.flags = flags;
            this.messageReceiver = messageReceiver;
            this.identitySystem = identitySystem;
            this.messageReceiver.OnEventCalled += OnQuestRequire;
            updateDailyQuestNow = true;
            Timmer().Forget();
        }


        async UniTask Timmer()
        {
            int prevHour = 0;
            int prevMin = 0;
            while (true)
            {
#if PRODUCTION
                if ((DateTime.UtcNow.Hour == 7 && prevHour == 6) || updateDailyQuestNow)
                {
                    updateDailyQuestNow = false;
                    (bool success, string res, string req, string path) ret = await APIServerConnector.GetDailyQuest();
                    Debug.Log(ret.path);
                    Debug.Log(ret.res);
                    Debug.Log(ret.req);
                    Debug.Log(ret.path);

                    APIRecovery.GetInstante().ReportAPIFailData(ret.path.Split("?")[0], ret.req, ret.res, ret.success);


                    dailyQuestData = ret.res;
                    Debug.Log("dailyQuestData" + dailyQuestData);
                }

                //Debug.Log("dailyQuestData" + dailyQuestData);

                prevHour = DateTime.UtcNow.Hour;
                //Debug.Log(prevHour);
                await UniTask.Delay(1000);

#endif

#if !PRODUCTION
                if ((DateTime.UtcNow.Minute % 10) == 0 && (prevMin % 10) == 9 || updateDailyQuestNow)
                {
                    updateDailyQuestNow = false;
                    (bool success, string res, string req, string path) ret = await APIServerConnector.GetDailyQuest();
                    Debug.Log(ret.path);
                    Debug.Log(ret.res);
                    Debug.Log(ret.req);
                    Debug.Log(ret.path);

                    APIRecovery.GetInstante().ReportAPIFailData(ret.path.Split("?")[0], ret.req, ret.res, ret.success);

                    dailyQuestData = ret.res;
                }

                prevMin = DateTime.UtcNow.Minute;
                //Debug.Log("dailyQuestData" + dailyQuestData);
                await UniTask.Delay(1000);
#endif
            }
        }

        private void OnQuestRequire(NetworkConnectionToClient connection, QuestMessage message, int channel)
        {
#if DEVELOPMENT
            Debug.Log("Server QRM received message: " + message.NetId);
            Debug.Log("Server QRM Find NetworkIdentity");
            Debug.Log("Require QRM *************************" + connection);
#endif

            var command = message.Message.Split(',')[0];
            if (command == "dialog")
            {
                var npcId = message.Message.Split(',')[1];
                //Debug.Log("[Quest]" + npcId);
                List<BaseData> dialogList = null;
                try
                {
                    dialogList = model.NpcDialogs[npcId];
                }
                catch
                {
#if DEVELOPMENT
                    //Debug.LogWarning("no Dialog for NPC");
#endif
                    connection.Send(new QuestMessage(message.NetId, "startdialogs," + 0));
                    connection.Send(new QuestMessage(message.NetId, "enddialogs," + 0));
                    return;
                }

                var questId = "";
                //Debug.Log("[Quest]" + dialogList.Count);
                connection.Send(new QuestMessage(message.NetId, "startdialogs," + dialogList.Count));

                for (int i = 0; i < dialogList.Count; i++)
                {
                    var acceptFlag = ((StartDialog)dialogList[i]).AcceptFlag.Split("■■");
                    var rejectFlag = ((StartDialog)dialogList[i]).RejectFlag.Split("■■");
#if DEVELOPMENT
                    //Debug.Log("---------QuestRunner Reject Flag---------");
#endif
                    var reject = false;
                    //loop all reject flag if found reject ignore this dialog
                    for (int j = 0; j < rejectFlag.Length; j++)
                    {
                        Debug.Log(rejectFlag[j]);
                        if ((flags.GetFlag(identitySystem[message.NetId].UID, rejectFlag[j]) != null))
                        {
                            reject = true;
                            break;
                        }

                    }
                    if (reject)
                    {
                        continue;
                    }

                    // Check must have flag
                    int starCount = 0;
                    for (int j = 0; j < acceptFlag.Length; j++)
                    {
                       // Debug.Log("[Quest]" + "1 " + acceptFlag.Length);
                       // Debug.Log("[Quest]" + "1 " + acceptFlag[j]);
                        if (acceptFlag[j].Length <= 0)
                        {
                            continue;
                        }
                        //this flag has *
                        if (acceptFlag[j][0] == '*')
                        {
                            starCount++;
                            if ((flags.GetFlag(identitySystem[message.NetId].UID, acceptFlag[j][1..]) == null))
                            {

                                reject = true;
                                break;
                            }

                        }
                    }

                    if (reject)
                    {
                        continue;
                    }
                    if (starCount > 0)
                    {
                        questId = SendStartQuestDialogToClient(connection, dialogList[i], message);
                        continue;
                    }

                    //loop all accepet flag if found accept do this
                    for (int j = 0; j < acceptFlag.Length; j++)
                    {
                      //  Debug.Log("[Quest]" + "2 " + acceptFlag.Length);
                      //  Debug.Log("[Quest]" + "2 " + acceptFlag[j]);
                        if ((flags.GetFlag(identitySystem[message.NetId].UID, acceptFlag[j]) != null))
                        {
                            Debug.Log(acceptFlag[j]);
                            questId = SendStartQuestDialogToClient(connection, dialogList[i], message);
                            break;
                        }

                    }

                }
               // Debug.Log("[Quest]" + questId);

                connection.Send(new QuestMessage(message.NetId, "enddialogs," + questId));
            }

            else if (command == "requestnextdialog")
            {
                var nextNode = message.Message.Split(',')[1];
                var questId = message.Message.Split(',')[2];
                SendQuestDialogToClient(connection, message, questId, nextNode);
            }

            else if (command == "dailyquest")
            {
                var addFlag = message.Message.Split(",")[1];
                Debug.Log(command + " quested " + addFlag);
                if (addFlag == "2_START")
                {
                    AddFlagManully(connection, message, addFlag);
                    APIServerConnector.StartQuest(identitySystem[message.NetId].AccessToken, "2").Forget();
                    APIServerConnector.SyncAPI(identitySystem[message.NetId].AccessToken).Forget();
                }

                return;
            }

        }

        public async Task AssignStartQuestFlag(NetworkConnectionToClient connection, uint netId, string uid, string command, string value = "", string accessToken = "", bool isFirstTime = false)
        {

            var flagReport = flags.Report(uid);

            if (!string.IsNullOrEmpty(accessToken))
            {
                if (isFirstTime)
                {
                    flags.Clear(uid);
                }

                List<UserFlag> flagMe = await APIServerConnector.GetMeFlag(accessToken);

                Debug.Log("[FlagLogIN] : " + flagMe.Count + " flagMe.Count");
                Debug.Log("[FlagLogIN] : " + JsonConvert.SerializeObject(flagMe));

                if (flagMe == null)
                {
                    Debug.Log("Player with " + netId + " is Disconnect");
                }

                else if (flagMe != null)
                {
                    if (flagMe.Count == 0) //if flagMe = 0 mean this is a 1st player
                    {
                        await flags.SetFlag(uid, "N6", "Hide Edward 6", "0");
                        await flags.SetFlag(uid, "N7", "Hide Edward 7", "0");
                        await flags.SetFlag(uid, "STARTER", "version0.1(alpha)", "0");
                        await APIServerConnector.StartQuest(accessToken, "1");
                        flagReport = flags.Report(uid);
                    }

                    else
                    {

                        for (int i = 0; i < flagMe.Count; i++)
                        {
                            if ((flags.GetFlag(uid, flagMe[i].flag.name) == null))
                            {

                                await flags.SetFlag(uid, flagMe[i].flag.name, flagMe[i].flag.name, "0", isFirstTime); // add flag

                            }

                        }

                        flagReport = flags.Report(uid);
                        if (flagReport == "ID Not Found")
                        {
                            flags.CreateFlags(uid);
                        }
                        flagReport = flags.Report(uid);
                        
                    }
                }

            }


#if DEVELOPMENT
            Debug.Log("value" + value);
#endif
            if (value == "")
            {
                connection.Send(new QuestMessage(netId, command + flagReport));
            }
            else
            {
                connection.Send(new QuestMessage(netId, command + "," + value));
            }



        }

        public string SendStartQuestDialogToClient(NetworkConnectionToClient connection, BaseData dialog, QuestMessage message)
        {
            QuestIdAndNodeId key = new QuestIdAndNodeId(((StartDialog)dialog).GetQuestId(), ((StartDialog)dialog).Next);
            DialogData data = new DialogData();
            data.Key = key;
            data.Dialog = (Dialog)model.NodeIdToDatas[key];
            var dialogJson = JsonConvert.SerializeObject(data);
            SetAndRemoveQuestFlag(connection, (Dialog)model.NodeIdToDatas[key], message);
            connection.Send(new QuestMessage(message.NetId, "adddialog," + dialogJson));
#if DEVELOPMENT
            Debug.Log("[Quest]addDialog" + ((StartDialog)dialog).GetQuestId());
#endif
            return ((StartDialog)dialog).GetQuestId();
        }

        public void SendQuestDialogToClient(NetworkConnectionToClient connection, QuestMessage message, string questId, string nextNode)
        {
            QuestIdAndNodeId key = new QuestIdAndNodeId(questId, nextNode);
            DialogData data = new DialogData();
            data.Key = key;
            data.Dialog = (Dialog)model.NodeIdToDatas[key];

            if (nextNode == "1f929e9a-7667-4f55-ad8c-2922a96a36f6638471471436505518" && questId == "2")
            {

                int[] randomNum = { 0, 1, 2 };

                for (int i = 0; i < 10; i++)
                {

                    var rnd1 = UnityEngine.Random.Range(0, randomNum.Length);
                    var rnd2 = UnityEngine.Random.Range(0, randomNum.Length);
                    var backUp = randomNum[rnd1];
                    randomNum[rnd1] = randomNum[rnd2];
                    randomNum[rnd2] = backUp;
                }


                var dailyQuestJObject = JsonConvert.DeserializeObject<JObject>(dailyQuestData);
#if DEVELOPMENT
                Debug.Log(dailyQuestData);
#endif

                var question = dailyQuestJObject["data"][0]["questions"];
                data.Dialog.Message = question["desc"].ToString();

                data.Dialog.Choices[randomNum[0]].Message = question["choices"][0]["desc"].ToString();
                data.Dialog.Choices[randomNum[0]].Next = correctNodeId;

                data.Dialog.Choices[randomNum[1]].Message = question["choices"][1]["desc"].ToString();
                data.Dialog.Choices[randomNum[1]].Next = wrongNodeId;

                data.Dialog.Choices[randomNum[2]].Message = question["choices"][2]["desc"].ToString();
                data.Dialog.Choices[randomNum[2]].Next = wrongNodeId;
            }

            var dialogJson = JsonConvert.SerializeObject(data);
#if DEVELOPMENT
            Debug.Log("dialogJson : " + dialogJson);
#endif
            SetAndRemoveQuestFlag(connection, (Dialog)model.NodeIdToDatas[key], message);
            connection.Send(new QuestMessage(message.NetId, "nextdialog," + dialogJson));
        }

        public async void AddFlagManully(NetworkConnectionToClient connection, QuestMessage message, string addFlag)
        {
            await flags.SetFlag(identitySystem[message.NetId].UID, addFlag, "0", "");
            FlagUpdate flagUpdate = new FlagUpdate();
            flagUpdate.uid = identitySystem[message.NetId].UID;
            flagUpdate.flag = addFlag;
            flagUpdate.value = "";
            await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "addflag", JsonConvert.SerializeObject(flagUpdate));
#if DEVELOPMENT
            Debug.Log("this to this");
#endif
            await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "finishupdateflag", addFlag);
#if DEVELOPMENT
            Debug.Log("this to 1");
#endif
        }

        public async void SetAndRemoveQuestFlag(NetworkConnectionToClient connection, Dialog dialogData, QuestMessage message)
        {

            var addFlag = (dialogData).AddFlag.Split("■■");
            var removeFlag = (dialogData).RemoveFlag.Split("■■");
            var questID = dialogData.GetQuestId();
#if DEVELOPMENT
            Debug.Log(addFlag.Length + " xxxxxx " + removeFlag.Length);
#endif

            if ((dialogData).AddFlag == "" && (dialogData).RemoveFlag == "")
            {
#if DEVELOPMENT
                Debug.Log("No Flag To Send");
#endif
                return;
            }

            await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "startupdateflag", (removeFlag.Length + addFlag.Length).ToString());
            for (int i = 0; i < addFlag.Length; i++)
            {
#if DEVELOPMENT
                Debug.Log("addFlag set flag : " + i);
#endif
                if (addFlag[i] == "")
                {
                    continue;
                }

                await flags.SetFlag(identitySystem[message.NetId].UID, addFlag[i], questID, questID);

                FlagUpdate flagUpdate = new FlagUpdate();
                flagUpdate.uid = identitySystem[message.NetId].UID;
                flagUpdate.flag = addFlag[i];
                flagUpdate.value = "";
                await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "addflag", JsonConvert.SerializeObject(flagUpdate));

            }

            for (int i = 0; i < removeFlag.Length; i++)
            {
                if (removeFlag[i] == "")
                {
                    continue;
                }

                await flags.Remove(identitySystem[message.NetId].UID, removeFlag[i]);
                FlagUpdate flagUpdate = new FlagUpdate();
                flagUpdate.uid = identitySystem[message.NetId].UID;
                flagUpdate.flag = removeFlag[i];
                flagUpdate.value = "";

                await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "removeflag", JsonConvert.SerializeObject(flagUpdate));

            }

            await AssignStartQuestFlag(connection, message.NetId, identitySystem[message.NetId].UID, "finishupdateflag", (removeFlag.Length + addFlag.Length).ToString(), identitySystem[message.NetId].AccessToken);
        }

    }

}
#endif