using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class Dialog : BaseData
    {
        [SerializeField]
        private string message;
        [SerializeField]
        private string addFlag;
        [SerializeField]
        private string removeFlag;
        [SerializeField]
        private List<Choice> choices;
        [SerializeField]
        private bool endQuest;

        [JsonProperty("message")]
        public string Message
        {
            get
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    return dialogNode.messageField.value;
                }
#endif
                return message;
            }
            set
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    dialogNode.messageField.value = value;
                }
#endif
                message = value;
            }
        }

        [JsonProperty("addFlag")]
        public string AddFlag
        {
            get
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {

                    return dialogNode.AddToString(dialogNode.itemListAddFlag);
                }
#endif
                return addFlag;
            }
            set
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    if (value != "")
                    {
                        dialogNode.ListViewAddFlag.itemsSource = (dialogNode.itemListAddFlag = dialogNode.AddToList(value));
                    }
                    else
                    {
                        dialogNode.ListViewAddFlag.itemsSource = dialogNode.itemListAddFlag;
                    }

                }
#endif
                addFlag = value;
            }
        }

        [JsonProperty("removeFlag")]
        public string RemoveFlag
        {
            get
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    return dialogNode.AddToString(dialogNode.itemListRemoveFlag);
                }
#endif
                return removeFlag;
            }
            set
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    if (value != "")
                    {
                        dialogNode.ListViewRemoveFlag.itemsSource = (dialogNode.itemListRemoveFlag = dialogNode.AddToList(value));
                    }
                    else
                    {
                        dialogNode.ListViewRemoveFlag.itemsSource = dialogNode.itemListRemoveFlag ;

                    }
                }
#endif
                removeFlag = value;
            }
        }

        [JsonProperty("choices")]
        public List<Choice> Choices { get => choices; set => choices = value; }

        [JsonProperty("completed")]
        public bool EndQuest
        {
            get
            {
                return endQuest;
            }
            set
            {
#if UNITY_EDITOR
                if (dialogNode != null)
                {
                    dialogNode.finishQuestToggle.value = value;
                }
#endif
                endQuest = value;
            }
        }
#if UNITY_EDITOR
        private DialogNode dialogNode;
        public Dialog()
        {
            choices = new List<Choice>();
        }
        public Dialog(DialogNode dialogNode)
        {
            this.dialogNode = dialogNode;
            choices = new List<Choice>();
        }
#endif

    }

}
