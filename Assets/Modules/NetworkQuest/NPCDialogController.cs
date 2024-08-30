using System.Collections;
using UnityEngine;
using Zenject;
using TMPro;
using com.playbux.tool;
using com.playbux.npc;
using UnityEngine.UI;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using Mirror;
using com.playbux.input;
using com.playbux.events;
using com.playbux.quest;
using com.playbux.ui.bubble;
using com.playbux.ui.world;
using com.playbux.ui;
using com.playbux.flag;
using com.playbux.identity;
using com.playbux.analytic;
using com.playbux.api;

namespace com.playbux.networkquest
{
    //FIXME: Refactor this to Conversation UI system
    public partial class NPCDialogController : MonoBehaviour
    {
#if !SERVER

        private IInputController inputController;

        [SerializeField]
        private GameObject choiceButtonPrefab;
        [SerializeField]
        private TextMeshProUGUI message;
        [SerializeField]
        private GameObject nextButton;
        [SerializeField]
        private GameObject choiceBox;
        [SerializeField]
        private GameObject dialogBox;
        [SerializeField]
        private TextMeshProUGUI[] npcNames;
        [SerializeField]
        private Image npcImage;
        [SerializeField]
        private Sprite orangeColorButton;

        private bool waitForSelect;
        private SignalBus signalBus;
        private IQuestRunner questRunner;
        private NPCDataBase npcDataBase;
        private CanvasGroup canvasGroup;
        private PlayerControls playerControls;
        private NPCDisplayer npcDisplayer;
        private List<GameObject> choiceGameobject;
        private Coroutine RunDialogCoroutine;
        private Coroutine RunSetTextCoroutine;
        private string activeNPC = "Not Found";
        private ConversationDialogSignal signal;
        private bool isWaiting = false;
        private IFlagCollection<string> flagCollectionBase;
        private float lastDownE = 0;
        private IdentitySystem IdentitySystem;
        //private BubbleController<InteractBubble.Pool> bubbleController;
        private InteractBallon interactBallon;
        private IBubble bubbleE;
        private Vector3 ballonOffest = new Vector3(-2.5f, 0, 2.5f);
        public bool WaitForSelect { get => waitForSelect; set => waitForSelect = value; }


        [Inject]
        void Setup(SignalBus signalBus, PlayerControls playerControls, IQuestRunner questRunner, NPCDataBase npcDataBase, IInputController inputController, InteractBallon interactBallon, NPCDisplayer npcDisplayer,
            IFlagCollection<string> flagCollectionBase, IdentitySystem IdentitySystem)
        {
            this.inputController = inputController;
            this.playerControls = playerControls;
            this.inputController.OnReleased += InteractNPC;
            this.questRunner = questRunner;
            this.npcDataBase = npcDataBase;
            //this.bubbleController = bubbleController;
            this.npcDisplayer = npcDisplayer;
            this.interactBallon = interactBallon;
            this.npcDataBase.CreateNPCDict();
            this.signalBus = signalBus;
            this.flagCollectionBase = flagCollectionBase;
            this.IdentitySystem = IdentitySystem;
            WaitForSelect = false;
            canvasGroup = transform.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.0f;
            choiceGameobject = new List<GameObject>();
            signal = new ConversationDialogSignal();
        }

        private void InteractNPC()
        {

            if (Time.time <= lastDownE + 1f)
            {

                return;
            }
            lastDownE = Time.time;


            if (WaitForSelect)
            {

                nextButton.GetComponent<ChoiceButtonScript>().OnClickNext();
                return;
            }

            if (NetworkClient.localPlayer == null)
            {
                return;
            }

            if (NetworkClient.localPlayer.transform == null)
            {
                return;
            }


            var playerPos = NetworkClient.localPlayer.transform.position;
            activeNPC = "Not Found";
            var nearestDistance = 1000000f;
            for (int i = 0; i < npcDataBase.AllNPCs.Count; i++)
            {
                //Debug.Log(NPCDataBase.Offset + npcDataBase.AllNPCs[i].NpcPosition + " Pos");

                if (!npcDisplayer.NpcDict[npcDataBase.AllNPCs[i].NpcId].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (Vector3.Distance(playerPos, NPCDataBase.Offset + npcDataBase.AllNPCs[i].NpcPosition) < nearestDistance)
                {
                    nearestDistance = Vector3.Distance(playerPos, NPCDataBase.Offset + npcDataBase.AllNPCs[i].NpcPosition);
                    activeNPC = npcDataBase.AllNPCs[i].NpcId;

                }

            }

            if (nearestDistance < 5)
            {
                if (activeNPC != "Not Found")
                {

                    SetNPCName(npcDataBase.NPCs[activeNPC].NpcName);
                    SetNPCImage(npcDataBase.NPCs[activeNPC].Sprite);
                    questRunner.CallNPC(activeNPC);
                    signalBus.Fire(signal);
                    playerControls.Chat.Disable();
                    playerControls.UI.Inventory.Disable();
                    return;
                }


            }

        }



        private void OnDestroy()
        {
            inputController.OnReleased -= InteractNPC;
        }

        public void Start()
        {


            /*bubbleE = bubbleController.GetBubble("E", Vector3.zero, BubbleChannel.Interaction);
            bubbleE.Hide();*/
            interactBallon.ChangePosition(Vector3.zero);
            interactBallon.Hide();
            HideDialog();
        }

        //Un Use For Now
        public void SyncServerFlag()
        {

            StartCoroutine(SyncFlags());
        }

        public bool HideDialog()
        {
            bool stateChange = false;
            try
            {
                if (canvasGroup.alpha != 0.0f)
                {
                    stateChange = true;
                }
            }
            catch
            {

            }
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return stateChange;
        }

        public void ShowDialog()
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

        }
        IEnumerator SyncFlags()
        {
            yield return new WaitUntil(() => NetworkClient.localPlayer != null);
            NetworkClient.Send(new QuestMessage(NetworkClient.localPlayer.netId, "flag," + NetworkClient.localPlayer.netId));
            HideDialog();

        }

        public void Update()
        {


            if (NetworkClient.localPlayer == null)
            {
                return;
            }

            if (NetworkClient.localPlayer.transform == null)
            {
                return;
            }



            var playerPos = NetworkClient.localPlayer.transform.position;


            var hideBubble = true;
            for (int i = 0; i < npcDataBase.AllNPCs.Count; i++)
            {

                if (Vector3.Distance(playerPos, NPCDataBase.Offset + npcDataBase.AllNPCs[i].NpcPosition) <= 5)
                {

                    if (!npcDisplayer.NpcDict[npcDataBase.AllNPCs[i].NpcId].gameObject.activeInHierarchy)
                    {
                        hideBubble = true;
                        break;
                    }

                    hideBubble = false;
                    interactBallon.ChangePosition(playerPos + ballonOffest);
                    break;
                }

            }

            if (hideBubble)
            {
                interactBallon.Hide();
            }
            else
            {
                interactBallon.Show();
            }


            if (activeNPC != "Not Found")
            {
                if (Vector3.Distance(playerPos, NPCDataBase.Offset + npcDataBase.NPCs[activeNPC].NpcPosition) > 5)
                {
                    try
                    {
                        if (HideDialog())
                        {
                            if (activeNPC == "3")
                            {
                                if (!string.IsNullOrEmpty(flagCollectionBase.GetFlag(IdentitySystem[NetworkClient.localPlayer.netId].UID, "2_START")))
                                {
                                    AnalyticWrapper.getInstance().Log("daily_quest_do",
                                        new LogParameter("user_id", PlayerPrefs.GetString(TokenUtility._id))
                                        , new LogParameter("action_type", "cancel")
                                         );
                                }
                            }
                        }
                    }
                    catch
                    {
                        HideDialog();
                    }

                    ClearDialog();
                    WaitForSelect = false;
                    playerControls.Chat.Enable();
                    playerControls.UI.Inventory.Enable();
                }

            }
        }


        //clear window
        public void ClearDialog()
        {
            message.text = string.Empty;
            if (choiceBox.transform.childCount != 0)
            {
                for (int i = 0; i < choiceGameobject.Count; i++)
                {
                    Destroy(choiceGameobject[i]);
                }
                choiceGameobject.Clear();
            }
            if (dialogBox.transform.childCount != 0)
            {
                for (int i = 0; i < dialogBox.transform.childCount; i++)
                {
                    Destroy(dialogBox.transform.GetChild(i).gameObject);
                }
            }
        }

        public void SetNPCImage(Sprite sprite)
        {
            npcImage.sprite = sprite;
        }

        public void SetNPCName(string name)
        {
            foreach (var npcName in npcNames)
            {
                npcName.text = name;
            }

        }

        public void AddDialog(Dialog dialog)
        {
            var dialogButton = Instantiate(choiceButtonPrefab, dialogBox.transform);
            dialogButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialog.Name;
            var choiceButtonScript = dialogButton.GetComponent<ChoiceButtonScript>();
            PassDialogData(choiceButtonScript, dialog.NodeID, dialog.GetQuestId());

        }

        //add button
        public void AddChoice(string text, string nextId, string questId, bool changeColor, Dialog dialog = null)
        {
            var choiceBut = Instantiate(choiceButtonPrefab, choiceBox.transform);
            choiceGameobject.Add(choiceBut);
            if (changeColor == true)
            {
                choiceBut.GetComponent<Image>().sprite = orangeColorButton;
            }
            var choiceButtonScript = choiceBut.GetComponent<ChoiceButtonScript>();

            foreach (var choiceText in choiceButtonScript.ChoiceTexts)
            {
                choiceText.text = text;
            }

            PassDialogData(choiceButtonScript, nextId, questId, dialog);

        }

        //set message text
        public void SetText(string messageText)
        {
            if (RunSetTextCoroutine != null)
            {
                StopCoroutine(RunSetTextCoroutine);
            }

            RunSetTextCoroutine = StartCoroutine(SetSpellText(messageText));
        }

        public void SetData(Dialog dialog, DialogLinkOutData dialogLinkOutData = null)
        {
            var messages = dialog.Message.Split("   ");
            //Debug.Log(messages.Length);

            if (RunDialogCoroutine != null)
            {
                StopCoroutine(RunDialogCoroutine);
            }

            RunDialogCoroutine = StartCoroutine(RunMessage(dialog, messages, dialogLinkOutData));
            return;

        }

        public void AssignNextButton(string nextId = "", string questId = "", Dialog dialog = null, DialogLinkOutData dialogLinkOutData = null)
        {
            var choiceButtonScript = nextButton.GetComponent<ChoiceButtonScript>();
            choiceButtonScript.NextId = nextId;
            choiceButtonScript.QuestId = questId;
            choiceButtonScript.Dialog = dialog;
            choiceButtonScript.DialogLinkOutData = dialogLinkOutData;
            choiceButtonScript.QuestRunner = questRunner;
            choiceButtonScript.NpcDialogController = this;
        }


        IEnumerator SetSpellText(string messageText)
        {

            for (int j = 0; j < messageText.Length; j++)
            {
                yield return new WaitForSecondsRealtime(0.01f);
                message.text = messageText[..(j + 1)];
            }

        }


        IEnumerator RunMessage(Dialog dialog, string[] messages, DialogLinkOutData dialogLinkOutData = null)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                ShowDialog();
                SetText(messages[i]);
                var showChoice = true;
                AssignNextButton("", "", dialog, dialogLinkOutData);
                if (i != (messages.Length - 1)) //ถ้าไม่ใช่ข้อความสุดท้ายของ Dialog ไม่ต้อง showChoice
                {
                    showChoice = false;
                }

                //Debug.Log(dialog.Choices);
                if (dialog.Choices == null) //ถ้าไม่มีข้อมูล Choice ไม่ต้อง showChoice
                {
                    showChoice = false;
                }

                if (dialog.Choices?.Count <= 0) //ถ้าไม่มีจำนวน Choice ไม่ต้อง showChoice
                {
                    showChoice = false;
                }

                if (dialog.Choices?.Count == 1 && dialog.Choices[0].Message == "") //ถ้ามีจำนวน Choice 1 Choiceและ Message ว่างเปล่า ไม่ต้อง showChoice
                {
                    showChoice = false;
                    AssignNextButton(dialog.Choices[0].Next, dialog.GetQuestId(), dialog);

                }

                nextButton.SetActive(true); //แสดงปุ่ม Next 
                WaitForSelect = true;

                if (showChoice)
                {
                    for (int j = 0; j < dialog.Choices.Count; j++)
                    {
                        if (j % 2 == 1)
                        {
                            AddChoice(dialog.Choices[j].Message, dialog.Choices[j].Next, dialog.GetQuestId(), true, dialog);
                            
                        }
                        else
                        {
                            AddChoice(dialog.Choices[j].Message, dialog.Choices[j].Next, dialog.GetQuestId(), false, dialog);
                        }


                    }

                    nextButton.SetActive(false);
                    yield return new WaitUntil(() => !WaitForSelect);
                }

                yield return null;
                yield return new WaitUntil(() => !nextButton.activeInHierarchy);
                //HideDialog();

            }

            HideDialog();
            ClearDialog();
            WaitForSelect = false;
            playerControls.Chat.Enable();
            playerControls.UI.Inventory.Enable();
        }



        private void PassDialogData(ChoiceButtonScript choiceButtonScript, string nextId, string questId, Dialog dialog = null)
        {
            choiceButtonScript.NextId = nextId;
            choiceButtonScript.QuestId = questId;
            choiceButtonScript.Dialog = dialog;
            choiceButtonScript.QuestRunner = questRunner;
            choiceButtonScript.NpcDialogController = this;
        }

        public void CallLinkOutSignalbus(DialogLinkOutData dialogLinkOutData)
        {
            signalBus.Fire(new LinkOutSignal(dialogLinkOutData));
        }
#endif
    }
}
