
using com.playbux.npc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.events;
using Zenject;
using Mirror;

namespace com.playbux.minimap
{
    public class LocationList : MonoBehaviour
    {

        public MiniMapLocator Locator;
        public GameObject BuildingBoxPrefab;
        public GameObject Content;
        public List<Toggle> ListToggle;

        [SerializeField]
        private Sprite questNpc;
        [SerializeField]
        private Sprite normalNpc;
        [SerializeField]
        private Sprite quizEvent;


        private IconGroup[] ToggleLink = { IconGroup.BUILDING, IconGroup.NPC, IconGroup.PLAYER, IconGroup.QUEST };

        private NPCDisplayer npcDisplayer;
        private SignalBus signalBus;
        private RefreshIconSignal refreshIconSignal;
        private FullMiniMapController fullMiniMapController;
#if !SERVER

        [Inject]
        void SetUp(NPCDisplayer npcDisplayer, SignalBus signalBus, FullMiniMapController fullMiniMapController)
        {

            this.signalBus = signalBus;
            this.npcDisplayer = npcDisplayer;
            this.fullMiniMapController = fullMiniMapController; 
            this.signalBus.Subscribe<RefreshIconSignal>(OnRefreshIconSignal);
        }

        public void OnRefreshIconSignal(RefreshIconSignal signal)
        {
            Debug.Log("GetSignal");
            StartCoroutine(WaitForUIRefesh());
        }

        public System.Predicate<MiniMapIcon> Filter()
        {
            var filter = IconGroup.NONE;
            for (int i = 0; i < ListToggle.Count; i++)
            {
                if (ListToggle[i].isOn)
                {
                    filter |= ToggleLink[i];
                }

            }

            filter |= IconGroup.PINPOINT;
            filter |= IconGroup.QUIZ;

            return p => (p.Group & filter) != IconGroup.NONE;
        }

        public void RefeshIcon()
        {

            Locator.Data.ClearFileter();
            Locator.Data.ClearAllCustomIcons();

            List<string> npcIdList = new List<string>();

            foreach (string npc in npcDisplayer.QuestNPCList)
            {
                npcIdList.Add(npc);
            }


            for (int i = 0; i < npcDisplayer.NpcDataBaseDict.Count; i++)
            {
                if (!npcDisplayer.NpcDataBaseDict.ContainsKey(i.ToString()))
                {
                    continue;
                }

                var npc = npcDisplayer.NpcDataBaseDict[i.ToString()];

                try
                {
                    npcIdList.First(n => n == npc.NpcId);
                    Locator.Data.AddCustomIcon(npc.NpcId, npc.NpcName, questNpc, npc.NpcPosition + NPCDataBase.Offset, true, IconGroup.QUEST);
                }
                catch
                {
                    if (npc.NpcId == "4")
                    {
                        Debug.Log("4");
                        Locator.Data.AddCustomIcon(npc.NpcId, npc.NpcName, quizEvent, npc.NpcPosition + NPCDataBase.Offset, true, IconGroup.QUIZ);
                    }
                    else 
                    {
                        Locator.Data.AddCustomIcon(npc.NpcId, npc.NpcName, normalNpc, npc.NpcPosition + NPCDataBase.Offset, true, IconGroup.NPC);
                    }
                        
                }

            }

            if (Content.transform.childCount > 0)
            {
                for (int i = Content.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(Content.transform.GetChild(i).gameObject);
                }
            }



            Locator.Data.AddFilter(Filter());
            for (int i = 0; i < Locator.Data.GetFilterLenght(); i++)
            {
                GameObject locationBox = Instantiate(BuildingBoxPrefab, Content.transform);
                var mapData = Locator.Data.GetFilterAt(i);
                locationBox.name = mapData.Name;
                var locationBoxScript = locationBox.GetComponent<LocationBox>();
                locationBoxScript.LocationImage.sprite = mapData.Icon;
                locationBoxScript.LocationName.text = mapData.DisplayName;
                locationBoxScript.LocationPosition = mapData.Position;
                locationBoxScript.LocationDisplay = mapData.Name;
                locationBoxScript.Locator = Locator;

            }

            
        }

        IEnumerator WaitForUIRefesh()
        {

            //yield return new WaitUntil(() => npcDisplayer.NpcDataBaseDict != null);
            //yield return new WaitUntil(() => npcDisplayer.NpcDataBaseDict.Count > 0);
            yield return new WaitUntil(() => Locator != null);
            yield return new WaitUntil(() => Locator.Data != null);
            yield return null;
            RefeshIcon();

        }
#endif
    }
}

