using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class QuestData : ScriptableObject
    {
        [SerializeField]
        private string questName;
        [SerializeField]
        private string questID;
        [SerializeField]
        private string position;
        [SerializeField]
        private List<NonPlayerCharacter> nonPlayerCharcater;
        [SerializeField]
        private List<StartDialog> startDialogs;
        [SerializeField]
        private List<Dialog> dialogs;
        [SerializeField]
        private List<QuestInformation> questInformation;
        [SerializeField]
        private List<string> allNodeIds; 
        [SerializeField]
        private List<string> allNodeDatas;
        

        [JsonProperty("questName")]
        public string QuestName { get => questName; set => questName = value; }

        [JsonProperty("questID")]
        public string QuestID { get => questID; set => questID = value; }

        [JsonProperty("position")]
        public string Position { get => position; set => position = value; }
        [JsonProperty("nonPlayerCharacter")]
        public List<NonPlayerCharacter> NonPlayerCharcater { get => nonPlayerCharcater; set => nonPlayerCharcater = value; }

        [JsonProperty("startDialogs")]
        public List<StartDialog> StartDialogs { get => startDialogs; set => startDialogs = value; }

        [JsonProperty("dialogNodes")]
        public List<Dialog> Dialogs { get => dialogs; set => dialogs = value; }

        [JsonProperty("questInformationNodes")]
        public List<QuestInformation> QuestInformation { get => questInformation; set => questInformation = value; }
        [JsonIgnore]
        public List<string> AllNodeIds { get => allNodeIds; set => allNodeIds = value; }
        [JsonIgnore]
        public List<string> AllNodeDatas { get => allNodeDatas; set => allNodeDatas = value; }
       
        
    }
}
