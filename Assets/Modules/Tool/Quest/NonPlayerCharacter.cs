using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class NonPlayerCharacter : BaseData
    {
        [SerializeField]
        private string nonPlayerCharacterID;
#if UNITY_EDITOR
        [SerializeField]
        private StartNodeList startDialogList;
#endif
        [SerializeField]
        private List<string> startDialogs;

        [JsonProperty("nonPlayerCharacterID")]
        public string NonPlayerCharacterID
        {
            get
            {
#if UNITY_EDITOR
                if (NonPlayerCharacterNode != null)
                {
                    return NonPlayerCharacterNode.nonPlayerCharacterBoxField.value;
                }
#endif
                return nonPlayerCharacterID;
            }
            set
            {
#if UNITY_EDITOR
                if (NonPlayerCharacterNode != null)
                {
                    NonPlayerCharacterNode.nonPlayerCharacterBoxField.value = value;
                }
#endif
                nonPlayerCharacterID = value;
            }

        }
        [JsonProperty("startDialogList")]
        public List<string> StartDialogList
        {
            get
            {
#if UNITY_EDITOR
                if (startDialogList != null)
                {
                    
                    return startDialogList.ToList();
                }
#endif
                return startDialogs;
            }
            set
            {
#if UNITY_EDITOR
                if (startDialogList != null)
                {
                    startDialogList.FromList(value);
                }
#endif
                startDialogs = value;
            }

        }


#if UNITY_EDITOR
        private NonPlayerCharacterNode NonPlayerCharacterNode;

        public NonPlayerCharacter(NonPlayerCharacterNode nonPlayerCharacterNode, StartNodeList startNodeList)
        {
            Apply(nonPlayerCharacterNode, startNodeList);
            this.startDialogs = new List<string>();
        }

        public NonPlayerCharacter()
        {
        }

        public void Apply(NonPlayerCharacterNode nonPlayerCharacterNode, StartNodeList startNodeList = null)
        {
            this.NonPlayerCharacterNode = nonPlayerCharacterNode;
            if(startNodeList != null)
            {
                this.startDialogList = startNodeList;
            }
            
            base.Apply(nonPlayerCharacterNode, "NonPlayerCharacterNode");
        }
#endif
    }
}