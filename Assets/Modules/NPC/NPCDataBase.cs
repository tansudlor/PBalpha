using com.playbux.ui;
using System.Collections.Generic;
using UnityEngine;
using static com.playbux.npc.NPCDataBase;

namespace com.playbux.npc
{
    [CreateAssetMenu(menuName = "Playbux/NPC/NPCDataBase", fileName = "NPCDataBase")]
    public class NPCDataBase : ScriptableObject
    {
        public List<NPCData> AllNPCs { get => allNPCs; set => allNPCs = value; }

        [SerializeField]
        private List<NPCData> allNPCs;
        public Dictionary<string, NPCData> NPCs { get; set; }
        public static Vector3 Offset { get => new Vector3(0, 0, 0); }
        public void CreateNPCDict()
        {
            NPCs = new Dictionary<string, NPCData>();

            for (int i = 0; i < AllNPCs.Count; i++)
            {
                NPCs[AllNPCs[i].NpcId] = AllNPCs[i];
            }
        }
    }

    [System.Serializable]
    public class NPCData
    {
        [SerializeField]
        private string npcId;
        [SerializeField]
        private string npcName;
        [SerializeField]
        private Vector3 npcPosition;
        [SerializeField]
        private string startDialog;
        [SerializeField]
        private string[] conditionDialog;
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private Sprite sprite;
        [SerializeField]
        private string hideFlag;
        [SerializeField]
        private string showQuestAvilable;
        [SerializeField]
        private DialogLinkOutData dialogData;

        public string NpcId { get => npcId; set => npcId = value; }
        public string NpcName { get => npcName; set => npcName = value; }
        public Vector3 NpcPosition { get => npcPosition; set => npcPosition = value; }
        public string StartDialog { get => startDialog; set => startDialog = value; }
        public GameObject Prefab { get => prefab; set => prefab = value; }
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public string HideFlag { get => hideFlag; set => hideFlag = value; }
        public string ShowQuestAvilable { get => showQuestAvilable; set => showQuestAvilable = value; }
        public DialogLinkOutData DialogData { get => dialogData; set => dialogData = value; }
        public string[] ConditionDialog { get => conditionDialog; set => conditionDialog = value; }
    }
}
