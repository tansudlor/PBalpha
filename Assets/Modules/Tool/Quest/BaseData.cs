using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using Node = UnityEditor.Experimental.GraphView.Node;
#endif

namespace com.playbux.tool
{
    [System.Serializable]
    public class BaseData
    {
        [SerializeField]
        private string title;
        [SerializeField]
        private string nodeID;
        [SerializeField]
        private string position;
        [SerializeField]
        private string nodeType;

        [JsonProperty("position")]
        public string Position { get => position; set => position = value; }

        /*[JsonProperty("name")]
        public string Name { get => title; set => title = value; }*/

        [JsonProperty("name")]
        public string Name
        {
            get
            {
                return title;
            }
            set 
            {
                title = value;
            }
        }

        [JsonProperty("nodeID")]
        public string NodeID { get => nodeID; set => nodeID = value; }

        [JsonProperty("nodeType")]
        public string NodeType { get => this.GetType().ToString(); set => nodeType = value; }

        //Only runtime field 
        private string questId;

        public string GetQuestId()
        {
            return questId;
        }
        public void SetQuestId(string Id)
        {
            questId = Id;
        }
#if UNITY_EDITOR
        public virtual void PrepareSave(Node node)
        {
            this.Position = node.GetPosition().x + "," + node.GetPosition().y;
        }
#endif
        public string InstanteType()
        {
            return nodeType;
        }
#if UNITY_EDITOR
        public virtual void Apply(Node infoNode, string serializeName)
        {
            this.GetType().GetField(serializeName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
            var props = this.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                this.GetType().GetField(serializeName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, null);
                var currentval = props[i].GetValue(this);
                this.GetType().GetField(serializeName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, infoNode);
                props[i].SetValue(this, currentval);
            }
        }

#endif
    }
}
