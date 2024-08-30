using com.playbux.tool;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.npc
{
    public interface INPCModel
    {
        public QuestData[] Quests { get; }
        public Dictionary<string, List<BaseData>> NpcDialogs { get; }
        public Dictionary<QuestIdAndNodeId, object> NodeIdToDatas { get; }
        //handle with json 
        string GetName(string npcId);
        Vector3 GetPosition(string npcId);
        List<string> GetNPCs(string npcId);
        public BaseData GetNodeById(QuestIdAndNodeId Id);
    }
}
