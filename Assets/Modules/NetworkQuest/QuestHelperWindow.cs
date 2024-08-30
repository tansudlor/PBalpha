using TMPro;
using Mirror;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.npc;
using com.playbux.flag;
using com.playbux.tool;
using System.Collections;
using com.playbux.identity;
using com.playbux.sfxwrapper;
using System.Collections.Generic;

namespace com.playbux.networkquest
{
    public class QuestHelperWindow : MonoBehaviour
    {
        [SerializeField]
        private GameObject contentMenu;
        [SerializeField]
        private GameObject contenMenuPrefab;
        [SerializeField]
        private GameObject questStep;
        [SerializeField]
        private GameObject questDescription;
        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject questHelperMenu;


        private IIdentitySystem identitySystem;
        private IFlagCollection<string> flagCollectionBase;
        private NPCModel npcModel;
        private List<QuestInformation> questInformation;
        private GameObject questStepInstance;
        private Coroutine descriptionEnum;

        private GameObject oldContentMenu = null;
        [Inject]
        void SetUp(IFlagCollection<string> flagCollectionBase, NPCModel npcModel, IIdentitySystem identitySystem)
        {
            this.flagCollectionBase = flagCollectionBase;
            this.npcModel = npcModel;
            this.identitySystem = identitySystem;
        }

        public void CreateDescription()
        {

            descriptionEnum = StartCoroutine(CreateDescriptionByStep());
        }


        public IEnumerator CreateDescriptionByStep()
        {
            yield return new WaitUntil(() => oldContentMenu == null);

            oldContentMenu = this.contentMenu;
            oldContentMenu.name = "WillDelete" + Time.time;
            var newContentMenu = Instantiate(contenMenuPrefab, oldContentMenu.gameObject.transform.parent);
            newContentMenu.name = "NewContent" + Time.time.ToString();
            scrollRect.content = newContentMenu.GetComponent<RectTransform>();
            yield return null;

            List<GameObject> questDescriptionList = new List<GameObject>();
            questStepInstance = null;
            questInformation = npcModel.QuestInfo;

            for (int i = 0; i < questInformation.Count; i++)//ทุก Node
            {
                questStepInstance = AddStep(questInformation[i].Name, newContentMenu);
                questStepInstance.SetActive(false);
                int finishCount = 0;

                for (int j = 0; j < questInformation[i].ProgressFlags.Count; j++) //ทุก Description ใน Node นี้
                {
                    var questDescription = questInformation[i].ProgressFlags[j].QuestDescription;
                    var finishFlag = questInformation[i].ProgressFlags[j].FinishFlag.Split("■■");
                    var activateFlag = questInformation[i].ProgressFlags[j].ActivateFlag.Split("■■");
                    var description = AddDescription(questDescription, questStepInstance.GetComponent<QuestStepController>().DescriptionContent);//FIXME: factory pattern
                    int activateCount = 0;

                    for (int k = 0; k < finishFlag.Length; k++)//ทุก finishFlag ใน Description นี้
                    {
                        var thisFlag = finishFlag[k];
                        if (thisFlag[0] == '*')
                        {
                            thisFlag = thisFlag[1..];
                        }
                        if ((flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, thisFlag) != null))
                        {
                            description.GetComponent<QuestDescription>().ChangeCheckBox();
                            description.GetComponent<TextMeshProUGUI>().color = Color.gray;
                            finishCount++;
                        }



                    }

                    for (int l = 0; l < activateFlag.Length; l++)// ทุก activateFlag ใน Description
                    {
#if DEVELOPMENT
                        Debug.Log("activateFlag.Length:" + activateFlag.Length);
#endif
                        var thisFlag = activateFlag[l];
                        if (thisFlag[0] == '*')
                        {
                            thisFlag = thisFlag[1..];
                        }

                        if ((flagCollectionBase.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, thisFlag) != null))
                        {

                            activateCount++;
#if DEVELOPMENT
                            Debug.Log("activateCount:" + activateCount);

#endif

                        }

                        if (activateCount >= activateFlag.Length)
                        {
                            questStepInstance.SetActive(true);
                        }


                    }


                }

                if (finishCount >= questInformation[i].ProgressFlags.Count)
                {
                    Destroy(questStepInstance);
                    continue;
                }
            }

            this.contentMenu = newContentMenu;
            yield return new WaitForSeconds(1.0f);
            yield return null;
            this.contentMenu.GetComponent<CanvasGroup>().alpha = 1.0f;
            Destroy(oldContentMenu);
            oldContentMenu = null;
        }

        GameObject AddStep(string stepName, GameObject parent)
        {
            var qsInstance = Instantiate(questStep, parent.transform);
            qsInstance.GetComponent<QuestStepController>().ChangeQuestName(stepName);
            return qsInstance;
        }

        GameObject AddDescription(string descriptionName, GameObject parent)
        {

            var descriptionInstance = Instantiate(questDescription, parent.transform);
            descriptionInstance.GetComponent<TextMeshProUGUI>().text = descriptionName;
            return descriptionInstance;
        }

        public void CloseQuestHelper()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            questHelperMenu.transform.localScale = Vector3.zero;
        }

        public void OpenQuestHelper()
        {
            if (questHelperMenu.transform.localScale == Vector3.zero)
            {
                questHelperMenu.transform.localScale = Vector3.one;
            }
            else
            {
                questHelperMenu.transform.localScale = Vector3.zero;
            }
        }

    }
}