
using Newtonsoft.Json;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class StartDialog : BaseData
    {
        [SerializeField]
        private string acceptFlag;
        [SerializeField]
        private string rejectFlag;
        [SerializeField]
        private string next;

        [JsonProperty("acceptFlag")]
        public string AcceptFlag
        {
            get
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    return startDialogNode.AddToString(startDialogNode.acceptFlagList);
                }
#endif
                return acceptFlag;
            }
            set
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    if (value != "")
                    {
                        startDialogNode.AcceptListView.itemsSource = (startDialogNode.acceptFlagList = startDialogNode.AddToList(value));
                    }
                    else
                    {
                        startDialogNode.AcceptListView.itemsSource = startDialogNode.acceptFlagList ;
                    }
                }
#endif
                acceptFlag = value;

            }
        }

        [JsonProperty("rejectFlag")]
        public string RejectFlag
        {
            get
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    return startDialogNode.AddToString(startDialogNode.rejectFlagList);
                }
#endif
                return rejectFlag;
            }
            set
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    if (value != "")
                    {
                        startDialogNode.RejectListView.itemsSource = (startDialogNode.rejectFlagList = startDialogNode.AddToList(value));
                    }
                    else
                    {
                        startDialogNode.RejectListView.itemsSource = startDialogNode.rejectFlagList;
                    }
                }
#endif
                rejectFlag = value;
            }
        }

        [JsonProperty("next")]
        public string Next
        {
            get
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    return startDialogNode.outputContainer[0].tooltip;
                }
#endif
                return next;
            }
            set
            {
#if UNITY_EDITOR
                if (startDialogNode != null)
                {
                    startDialogNode.outputContainer[0].tooltip = value;
                }
#endif
                next = value;
            }
        }

#if UNITY_EDITOR
        private StartDialogNode startDialogNode;
        public StartDialog(StartDialogNode startDialogNode)
        {
            this.startDialogNode = startDialogNode;
        }
        public StartDialog()
        {

        }
#endif
    }
}


