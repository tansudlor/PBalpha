using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.map;
using com.playbux.tool;
using com.playbux.flag;
using com.playbux.sorting;
using com.playbux.identity;
using System.Collections.Generic;

namespace com.playbux.npc
{
    public class ArrowAngleFactory : PlaceholderFactory<ArrowAngleController>
    {
        List<ArrowAngleController> arrows;

        public ArrowAngleFactory()
        {
            arrows = new List<ArrowAngleController>();
        }

        public override ArrowAngleController Create()
        {
            ArrowAngleController arrowAngleController = base.Create();
            arrows.Add(arrowAngleController);
            return arrowAngleController;
        }

        public void Clear()
        {
            foreach (var item in arrows)
            {
                GameObject.Destroy(item.gameObject);
            }

            arrows.Clear();
        }
    }

    public interface INPCDirectionObserver
    {
        public void OnNPCUpdate(HashSet<string> npc, NPCDataBase npcInfo);
    }


    public class NPCDirectionObserver : INPCDirectionObserver
    {
        [SerializeField] private GameObject arrow;


        private ArrowAngleFactory arrowAngleFactory;

        public NPCDirectionObserver(ArrowAngleFactory arrowAngleFactory)
        {
            this.arrowAngleFactory = arrowAngleFactory;
        }

        public void OnNPCUpdate(HashSet<string> npcIds, NPCDataBase npcInfo)
        {
            foreach (var item in npcIds)
            {
                ArrowAngleController arrow = arrowAngleFactory.Create();
                arrow.Npc = npcInfo.NPCs[item];
            }
        }
    }

    public class NPCDisplayer
    {
        public Dictionary<string, GameObject> NpcDict
        {
            get => npcDict;
            set => npcDict = value;
        }

        public HashSet<string> QuestNPCList
        {
            get => questNPCList;
            set => questNPCList = value;
        }

        public bool NpcChange
        {
            get => npcChange;
            set => npcChange = value;
        }
        public Dictionary<string, NPCData> NpcDataBaseDict
        {
            get => npcDataBaseDict;
            set => npcDataBaseDict = value;
        }

        private readonly NPCDataBase data;
        private readonly NPCModel npcModel;
        private readonly IMapController mapController;
        private readonly IIdentitySystem identitySystem;
        private readonly NPCSorter.Factory npcSorterFactory;
        private readonly ArrowAngleFactory arrowAngleFactory;
        private readonly IFlagCollection<string> flagCollectionBase;
        private readonly LayerSorterController layerSorterController;
        private readonly INPCDirectionObserver npcDirectionController;

        private bool npcChange;
        private string currentMapName;
        private HashSet<string> questNPCList;
        private Dictionary<string, GameObject> npcDict;
        private Dictionary<string, NPCData> npcDataBaseDict;

        public NPCDisplayer(
            NPCDataBase data,
            NPCModel npcModel,
            IMapController mapController,
            IIdentitySystem identitySystem,
            NPCSorter.Factory npcSorterFactory, 
            ArrowAngleFactory arrowAngleFactory, 
            IFlagCollection<string> flagCollectionBase,
            INPCDirectionObserver npcDirectionController,
            LayerSorterController layerSorterController)
        {
            this.data = data;
            this.npcModel = npcModel;
            this.mapController = mapController;
            this.identitySystem = identitySystem;
            this.npcSorterFactory = npcSorterFactory;
            this.arrowAngleFactory = arrowAngleFactory;
            this.flagCollectionBase = flagCollectionBase;
            this.layerSorterController = layerSorterController;
            this.npcDirectionController = npcDirectionController;
            

            NpcDict = new Dictionary<string, GameObject>();
            QuestNPCList = new HashSet<string>();
            NpcDataBaseDict = new Dictionary<string, NPCData>();

            this.data.CreateNPCDict();

            this.mapController.OnCreated += CreateNPC;
        }

        public void CreateNPC(string mapName)
        {
            if (currentMapName == mapName)
                return;

            if (string.IsNullOrEmpty(mapName))
                return;

            for (int i = 0; i < data.AllNPCs.Count; i++)
            {
                var npc = npcSorterFactory.Create(data.AllNPCs[i].Prefab, NPCDataBase.Offset + data.AllNPCs[i].NpcPosition);
                NpcDict[data.AllNPCs[i].NpcId] = npc.gameObject;
                layerSorterController.Add(npc.Sortable);
            }
        }

        public void DisplayAvalibleNPC()
        {
            QuestNPCList.Clear();
            arrowAngleFactory.Clear();

            for (int i = 0; i < data.AllNPCs.Count; i++)
            {
                var hideFlag = data.AllNPCs[i].HideFlag;

                if (flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, hideFlag) != null)
                {
                    NpcDict[data.AllNPCs[i].NpcId].SetActive(false);
                }

                else if (flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, hideFlag) == null)
                {
                    NpcDict[data.AllNPCs[i].NpcId].SetActive(true);
                    NpcDataBaseDict[i.ToString()] = data.AllNPCs[i];
                    ShowExclamation(data.AllNPCs[i].NpcId);

                }
            }

            npcDirectionController.OnNPCUpdate(QuestNPCList, data);
        }

        public void ShowExclamation(string npcId)
        {
            
            NpcDict[npcId].transform.GetChild(0).gameObject.SetActive(false);
            List<BaseData> dialogList = new List<BaseData>();
            if (!npcModel.NpcDialogs.ContainsKey(npcId) )
            {
                return;
            }
            else
            {
                dialogList = npcModel.NpcDialogs[npcId];
            }


            //TODO: Check Marker Piority before show to game  /next phase
            for (int i = 0; i < dialogList.Count; i++)
            {
                var acceptFlag = ((StartDialog)dialogList[i]).AcceptFlag.Split("■■");
                var rejectFlag = ((StartDialog)dialogList[i]).RejectFlag.Split("■■");
                var reject = false;

                for (int j = 0; j < rejectFlag.Length; j++)
                {
                    if ((flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID,
                            rejectFlag[j]) != null))
                    {
                        reject = true;
                        break;
                    }
                }

                if (reject)
                {
                    continue;
                }

                // Check must have flag
                int starCount = 0;
                for (int j = 0; j < acceptFlag.Length; j++)
                {
                    if (acceptFlag[j].Length <= 0)
                    {
                        continue;
                    }

                    //this flag has *
                    if (acceptFlag[j][0] == '*')
                    {
                        starCount++;
                        if ((flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID,
                                acceptFlag[j][1..]) == null))
                        {
                            reject = true;
                            break;
                        }
                    }
                }

                if (reject)
                {
                    continue;
                }


                if (starCount > 0)
                {
                    ShowMarker(npcId, dialogList[i].Name);
                    return;
                }

                //loop all accepet flag if found accept do this
                for (int j = 0; j < acceptFlag.Length; j++)
                {
                    if ((flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID,
                            acceptFlag[j]) != null))
                    {
                        ShowMarker(npcId, dialogList[i].Name);
                        return;
                    }
                }
            }
        }

        public void ShowMarker(string npcId, string dialogName)
        {
#if DEVELOPMENT
            Debug.Log(dialogName);
#endif

            if (dialogName[0] == '!')
            {
                QuestNPCList.Add(npcId);
                NpcDict[npcId].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                NpcDict[npcId].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}