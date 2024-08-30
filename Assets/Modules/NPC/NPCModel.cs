
using com.playbux.tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.playbux.npc
{
    public class NPCModel : INPCModel
    {
        private QuestData[] quests;
        private Dictionary<string, List<BaseData>> npcDialogs;
        private Dictionary<QuestIdAndNodeId, object> nodeIdToDatas;
        private List<QuestInformation> questInfo;

        public QuestData[] Quests { get => quests;  }
        public Dictionary<string, List<BaseData>> NpcDialogs { get => npcDialogs;  }
        public Dictionary<QuestIdAndNodeId, object> NodeIdToDatas { get => nodeIdToDatas;  }
        public List<QuestInformation> QuestInfo { get => questInfo; set => questInfo = value; }

        public NPCModel(QuestData[] quests)
        {
            this.quests = quests;
            npcDialogs = new Dictionary<string, List<BaseData>>();
            nodeIdToDatas = new Dictionary<QuestIdAndNodeId, object>();
            questInfo = new List<QuestInformation>();
            //All Quest
            for (int i = 0; i < quests.Length; i++)
            {
                for (int j = 0; j < quests[i].AllNodeIds.Count; j++)
                {
                    string assemblyName = "com.playbux.tool";
                    string nodeType = JsonConvert.DeserializeObject<BaseData>(quests[i].AllNodeDatas[j]).InstanteType() + ", " + assemblyName;
                    Type targetType = Type.GetType(nodeType);
                    QuestIdAndNodeId key = new QuestIdAndNodeId();
                    key.QuestId = quests[i].QuestID;
                    key.NodeId = quests[i].AllNodeIds[j];
                    NodeIdToDatas[key] = JsonConvert.DeserializeObject(quests[i].AllNodeDatas[j], targetType);
                    ((BaseData)NodeIdToDatas[key]).SetQuestId(quests[i].QuestID);
                }
            }

            //All Quest
            for (int i = 0; i < quests.Length; i++)
            {
                //Each Quest NPC
                for (int j = 0; j < quests[i].NonPlayerCharcater.Count; j++)
                {
                    string npcID = quests[i].NonPlayerCharcater[j].NonPlayerCharacterID;

                    if (string.IsNullOrEmpty(npcID))
                    {
                        continue;
                    }

                    if (!NpcDialogs.ContainsKey(npcID))
                    {
                        NpcDialogs[npcID] = new List<BaseData>();
                    }

                    //Each NPC Start Dialog
                    for (int k = 0; k < quests[i].NonPlayerCharcater[j].StartDialogList.Count; k++)
                    {
                        var key = new QuestIdAndNodeId();
                        key.QuestId = quests[i].QuestID;
                        key.NodeId = quests[i].NonPlayerCharcater[j].StartDialogList[k];
                        NpcDialogs[npcID].Add(GetNodeById(key));
                    }

                }

                //All QuestInformation
                for(int l = 0;l < quests[i].QuestInformation.Count; l++)
                {
                    
                    questInfo.Add(quests[i].QuestInformation[l]);
                }
            }
            
        }


        public BaseData GetNodeById(QuestIdAndNodeId Id)
        {

            return (BaseData)NodeIdToDatas[Id];
        }

        public string GetName(string npcId)
        {
            throw new NotImplementedException();
        }

        public List<string> GetNPCs(string npcId)
        {
            return NpcDialogs.Keys.ToList();
        }

        public Vector3 GetPosition(string npcId)
        {
            throw new NotImplementedException();
        }
    }
}
