using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class QuestInformation : BaseData
    {
        [SerializeField]
        private string nextInfo;
        [SerializeField]
        private List<ProgressFlag> progressFlags;

        [JsonProperty("nextInfo")]
        public string NextInfo { get => nextInfo; set => nextInfo = value; }

        [JsonProperty("progressFlags")]
        public List<ProgressFlag> ProgressFlags { get => progressFlags; set => progressFlags = value; }

#if UNITY_EDITOR
        private QuestInfoNode questInfoNode;
        public QuestInformation()
        {
            progressFlags = new List<ProgressFlag>();
        }
        public QuestInformation(QuestInfoNode questInfoNode)
        {
            this.questInfoNode = questInfoNode;
            progressFlags = new List<ProgressFlag>();
        }
#endif
    }
    [System.Serializable]
    public class ProgressFlag
    {
        [SerializeField]
        private string questDescription;
        [SerializeField]
        private string activateFlag;
        [SerializeField]
        private string finishFlag;

        [JsonProperty("questDescription")]
        public string QuestDescription { get => questDescription; set => questDescription = value; }

        [JsonProperty("activteFlag")]
        public string ActivateFlag { get => activateFlag; set => activateFlag = value; }

        [JsonProperty("finishFlag")]
        public string FinishFlag { get => finishFlag; set => finishFlag = value; }
    }

}
