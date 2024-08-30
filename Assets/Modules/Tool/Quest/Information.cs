
using System.Collections.Generic;
namespace com.playbux.tool
{
    [System.Serializable]
    public class Information : BaseData
    {
        private string questName;
        private string questID;
        public string QuestName
        {
            get
            {
#if UNITY_EDITOR
                if (informationNode != null)
                {
                    return informationNode.QuestNametextField.value;
                }
#endif
                return questName;
            }
            set
            {
#if UNITY_EDITOR
                if (informationNode != null)
                {
                    informationNode.QuestNametextField.value = value;
                }
#endif
                questName = value;
            }
        }
        public string QuestID
        {
            get
            {
#if UNITY_EDITOR
                if (informationNode != null )
                {
                   
                    return informationNode.QuestIdtextField.value;
                }
#endif
                return questID;
            }
            set
            {
#if UNITY_EDITOR
                if (informationNode != null)
                {
                    informationNode.QuestIdtextField.value = value;
                }
#endif
                questID = value;
            }
        }

        public List<string> NonPlayerCharacterIDs { get; set; }

#if UNITY_EDITOR
        private InformationNode informationNode;
        public Information(InformationNode informationNode)
        {
            this.informationNode = informationNode;  
        }

        

        public Information()
        {

        }
#endif
    }
}
