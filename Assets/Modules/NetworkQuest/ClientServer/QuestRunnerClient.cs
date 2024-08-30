#if !SERVER
using com.playbux.networking.mirror.message;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using com.playbux.tool;
using com.playbux.npc;
using com.playbux.flag;
using com.playbux.quest;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using Zenject;
using com.playbux.events;
using System;
using com.playbux.functioncollection;
using com.playbux.ui;
using com.playbux.api;
using com.playbux.analytic;

namespace com.playbux.networkquest
{
    public sealed partial class QuestRunner : IQuestRunner
    {
        private NPCDialogController npcDialogController;
        private INetworkMessageReceiver<QuestMessage> messageReceiver;
        private NPCDataBase npcInfo;
        private List<Dialog> availableDialog;
        private string questId;
        private string lastNpcId;
        private IFlagCollection<string> flagCollection;
        private NPCDisplayer npcDisplayer;
        private QuestHelperWindow questHelperWindow;
        private IIdentitySystem identitySystem;
        private SignalBus signalBus;
        private Dictionary<string, Action> functionCall;
        public Dictionary<string, Action> FunctionCall => functionCall;

        public QuestRunner(INetworkMessageReceiver<QuestMessage> messageReceiver, NPCDataBase npcData,
            NPCDisplayer npcDisplayer,
            IFlagCollection<string> flagCollectionBase, QuestHelperWindow questHelperWindow,
            NPCDialogController npcDialogController, IIdentitySystem identitySystem
            , SignalBus signalBus)
        {
            this.signalBus = signalBus;
            this.messageReceiver = messageReceiver;
            this.questHelperWindow = questHelperWindow;
            npcInfo = npcData;
            npcInfo.CreateNPCDict();
            this.npcDisplayer = npcDisplayer;
            this.identitySystem = identitySystem;
            this.messageReceiver.OnEventCalled += OnQuestResponse;
            this.flagCollection = flagCollectionBase;
            this.npcDialogController = npcDialogController;
            functionCall = new Dictionary<string, Action>();
        }



        public void CallNPC(string id)
        {

            // Debug.Log("QRM Test Call Dialog");
            lastNpcId = id;
            GetDialog(id);
        }

        public void GetDialog(string npcId)
        {
            //Debug.Log("dialog," + npcId);
            NetworkClient.Send(new QuestMessage(NetworkClient.localPlayer.netId, "dialog," + npcId));
        }

        public void GetNext(string nextDialog, string questId)
        {
            NetworkClient.Send(new QuestMessage(NetworkClient.localPlayer.netId,
                "requestnextdialog," + nextDialog + "," + questId));
        }

        private void OnQuestResponse(QuestMessage message)
        {
            //Debug.Log("QRM: " + message.Message);
            //("startdialogs,n") n = startdialog count
            var command = message.Message.Split(',')[0];

            if (command == "startdialogs")
            {
                availableDialog = new List<Dialog>();
                //Debug.Log(message.Message);
                return;
            }

            else if (command == "enddialogs")
            {
                npcDialogController.ClearDialog();
                //Debug.Log("Quest Count :" + availableDialog.Count);
                //TODO: if availableDialog = 1 go ;
                if (availableDialog.Count > 1)
                {
                    npcDialogController.ShowDialog();
                    for (int i = 0; i < availableDialog.Count; i++)
                    {
                        var dialogChoice = availableDialog[i];

                        npcDialogController.SetText("Select Conversation");
                        npcDialogController.AddDialog(dialogChoice);
                    }
                }
                else if (availableDialog.Count == 1)
                {
                    npcDialogController.ShowDialog();
                    var dialog = availableDialog[0];
                    try
                    {
                        if (dialog.Name == "WrongAnswer")
                        {
                            AnalyticWrapper.getInstance().Log("daily_quest_do",
                             new LogParameter("user_id", TokenUtility._id)
                                 , new LogParameter("action_type", "answer")
                                    );
                        }
                        if (dialog.Name == "CorrectAnswer")
                        {
                            AnalyticWrapper.getInstance().Log("daily_quest_do",
                             new LogParameter("user_id", TokenUtility._id)
                                 , new LogParameter("action_type", "answer")
                                    );

                            AnalyticWrapper.getInstance().Log("daily_quest_completed",
                             new LogParameter("user_id", TokenUtility._id)
                                 , new LogParameter("total_pebble", "5")
                                    );
                        }
                        if (dialog.EndQuest == true && dialog.GetQuestId() == "1")
                        {
                            AnalyticWrapper.getInstance().Log("welcome_quest_completed",
                             new LogParameter("user_id", TokenUtility._id)
                                 , new LogParameter("event_name", "playbux_shop")
                                    );
                        }
                        string event_name = dialog.Name.Split(",")[1];
                        AnalyticWrapper.getInstance().Log("welcome_quest_do",
                             new LogParameter("user_id", TokenUtility._id)
                                 , new LogParameter("event_name", event_name)
                                    );
                    }
                    catch
                    {

                    }

                    npcDialogController.ClearDialog();
                    npcDialogController.SetData(dialog);
                }
                else
                {
                    var npcDict = npcInfo.NPCs;
                    NPCData npcData = null;
                    if (npcDict.ContainsKey(lastNpcId))
                    {
                        npcData = npcDict[lastNpcId];
                    }
                    else
                    {
                        //Debug.LogError($"No data found for NpcId: {lastNpcId}");
                    }



                    Dialog dialog = new Dialog();
                    Debug.Log(JsonConvert.SerializeObject(dialog));

                    if (npcData.DialogData != null)
                    {
                        signalBus.Fire(new LinkOutSignal(npcData.DialogData));
                        return;
                    }

                    npcDialogController.ShowDialog();
                    npcDialogController.ClearDialog();

                    dialog.Message = npcData.StartDialog;
                    
                    try
                    {
                        foreach (var item in npcData.ConditionDialog)
                        {
                            var condition = item.Split("=");
                            
                            if (flagCollection.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, condition[0]) != null)
                            {
                                dialog.Message = condition[1];
                            }
                        }
                    }
                    catch
                    {
                        dialog.Message = npcData.StartDialog;
                    }

                    npcDialogController.SetData(dialog);


                }


                return;
            }

            else if (command == "adddialog")
            {
                var jsonData = message.Message[(message.Message.IndexOf(',') + 1)..];
                var dialogData = (JsonConvert.DeserializeObject<DialogData>(jsonData));
                dialogData.Dialog.SetQuestId(dialogData.Key.QuestId);
                availableDialog.Add(dialogData.Dialog);
                return;
            }
            else if (command == "nextdialog")
            {
                var jsonData = message.Message[(message.Message.IndexOf(',') + 1)..];
                var dialogData = JsonConvert.DeserializeObject<DialogData>(jsonData);
                try
                {
                    if (dialogData.Dialog.Choices[0].Next == "63d9c779-a251-4455-ba9b-826a0ebf1279638499829872787033")
                    {
                        Debug.Log(jsonData);
                    }
                }
                catch
                {

                }
                dialogData.Dialog.SetQuestId(dialogData.Key.QuestId);
                var dialog = dialogData.Dialog;
                npcDialogController.ClearDialog();
                npcDialogController.SetData(dialog);
                return;
            }
            else if (command == "flagcollection")
            {
                Debug.Log("flagcollection " + message);
                WaitForUID(message).Forget();
                return;
            }

            else if (command == "addflag")
            {
                var flagData = message.Message[(message.Message.IndexOf(',') + 1)..];
                FlagUpdate flagUpdate = JsonConvert.DeserializeObject<FlagUpdate>(flagData);

                Debug.Log(flagUpdate.uid +  "  " + flagUpdate.flag);

                flagCollection.SetFlag(flagUpdate.uid, flagUpdate.flag, flagUpdate.value, "0");

            }

            else if (command == "removeflag")
            {
                var flagData = message.Message[(message.Message.IndexOf(',') + 1)..];
                FlagUpdate flagUpdate = JsonConvert.DeserializeObject<FlagUpdate>(flagData);
                flagCollection.Remove(flagUpdate.uid, flagUpdate.flag);
                //set new npc here
            }
            else if (command == "finishupdateflag")
            {
                var flagData = message.Message.Split(',')[1];
                // Debug.Log(flagData + " finishupdateflag");
                npcDisplayer.DisplayAvalibleNPC();
                questHelperWindow.CreateDescription();
                signalBus.Fire(new RefreshIconSignal());
            }
        }

        private async UniTask WaitForUID(QuestMessage message)
        {
            await UniTask.WaitUntil(() => !string.IsNullOrEmpty(identitySystem[message.NetId].UID));
            PlayerPrefs.SetString(TokenUtility._id, identitySystem[message.NetId].UID);
            var flagData = message.Message[(message.Message.IndexOf(',') + 1)..];

            //Debug.Log("flagData " + flagData);

            List<FlagUpdate> flagUpdateList = JsonConvert.DeserializeObject<List<FlagUpdate>>(flagData);

            for (int i = 0; i < flagUpdateList.Count; i++)
            {
                Debug.Log(flagUpdateList[i].flag + " flag");
            }


            for (int i = 0; i < flagUpdateList.Count; i++)
            {
                await flagCollection.SetFlag(flagUpdateList[i].uid, flagUpdateList[i].flag, flagUpdateList[i].value, "0");
            }

            await flagCollection.SetFlag(identitySystem[message.NetId].UID, "N4", "Mr. Curious", "0");

            npcDisplayer.DisplayAvalibleNPC();
            questHelperWindow.CreateDescription();
            signalBus.Fire(new RefreshIconSignal());
        }


    }
}
#endif