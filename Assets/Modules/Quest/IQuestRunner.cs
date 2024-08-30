using com.playbux.networking.mirror.message;
using com.playbux.tool;
using Mirror;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.playbux.quest
{
    public interface IQuestRunner
    {
#if !SERVER
        public void CallNPC(string id);
        public void GetDialog(string npcId);
        public void GetNext(string nextDialog, string questId);
        public Dictionary<string, Action> FunctionCall { get; }
#endif
#if SERVER
        public Task AssignStartQuestFlag(NetworkConnectionToClient connection, uint netId, string uid, string command, string value = "", string accessToken = "", bool isFirstTime = false);
        public void SetAndRemoveQuestFlag(NetworkConnectionToClient connection, Dialog dialogData, QuestMessage message);
        public string SendStartQuestDialogToClient(NetworkConnectionToClient connection, BaseData dialog, QuestMessage message);
        public void SendQuestDialogToClient(NetworkConnectionToClient connection, QuestMessage message, string questId, string nextNode);
#endif

    }
}