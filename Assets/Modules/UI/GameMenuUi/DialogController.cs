
using com.playbux.networkquest;
using com.playbux.quest;
using com.playbux.tool;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui.gamemenu
{
    public class DialogController : MonoBehaviour
    {
#if !SERVER
        [SerializeField]
        private GameObject choiceButtonPrefab;
        [SerializeField]
        private GameObject choiceDotPrefab;
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
        private Sprite[] colorButton;
        [SerializeField]
        private Color[] colorDots;
        [SerializeField]
        private CanvasGroup canvasGroup;

        private List<GameObject> choiceGameobject;
        private Coroutine RunDialogCoroutine;
        private Coroutine RunSetTextCoroutine;

        public bool waitForSelect;

        private void Start()
        {
            canvasGroup.alpha = 0.0f;
            choiceGameobject = new List<GameObject>();
        }

        public void DialogData(Sprite npcSprite, string npcName, string messgae, string[] choices = null, string[] nodeIds = null)
        {
            ClearDialog();
            HideDialog();
            ShowDialog();
            SetNPCName(npcName);
            SetNPCImage(npcSprite);

            Dialog dialog = new Dialog();
            List<Choice> choicesDialogList = new List<Choice>();
            dialog.Message = messgae;

            if (choices != null)
            {
                for (int i = 0; i < choices.Length; i++)
                {
                    Choice choiceDialog = new Choice();
                    choiceDialog.Message = choices[i];
                    if (nodeIds != null)
                    {
                        choiceDialog.Next = nodeIds[i];
                    }
                    else
                    {
                        choiceDialog.Next = choiceDialog.Message;
                    }

                    choicesDialogList.Add(choiceDialog);

                }

                dialog.Choices = choicesDialogList;
            }


            SetData(dialog);
        }


        public void ClearData()
        {
            ClearDialog();
            HideDialog();

        }
        private void SetData(Dialog dialog)
        {
            var messages = dialog.Message.Split("   ");

            if (RunDialogCoroutine != null)
            {
                StopCoroutine(RunDialogCoroutine);
            }

            RunDialogCoroutine = StartCoroutine(RunMessage(dialog, messages));
            return;

        }

        private void HideDialog()
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void ShowDialog()
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

        }

        private void ClearDialog()
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


        private void SetNPCImage(Sprite sprite)
        {
            npcImage.sprite = sprite;
        }

        private void SetNPCName(string name)
        {
            foreach (var npcName in npcNames)
            {
                npcName.text = name;
            }

        }

        private void AddChoice(string text, string nextId, Sprite buttonSprite, string questId = null)
        {
            var choiceBut = Instantiate(choiceButtonPrefab, choiceBox.transform);
            choiceGameobject.Add(choiceBut);

            choiceBut.GetComponent<Image>().sprite = buttonSprite;

            var choiceButtonScript = choiceBut.GetComponent<ChoiceButtonScript>();

            foreach (var choiceText in choiceButtonScript.ChoiceTexts)
            {
                choiceText.text = text;
            }

            if (text == nextId)
            {
                var button = choiceBut.GetComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.interactable = false;
            }
            else
            {
                PassDialogData(choiceButtonScript, nextId, questId);
            }


        }

        private void SetText(string messageText)
        {
            if (RunSetTextCoroutine != null)
            {
                StopCoroutine(RunSetTextCoroutine);
            }

            RunSetTextCoroutine = StartCoroutine(SetSpellText(messageText));
        }

        private IEnumerator SetSpellText(string messageText)
        {

            for (int j = 0; j < messageText.Length; j++)
            {
                yield return new WaitForSecondsRealtime(0.01f);
                message.text = messageText[..(j + 1)];
            }

        }

        private IEnumerator RunMessage(Dialog dialog, string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                ShowDialog();
                SetText(messages[i]);
                var showChoice = true;
                AssignNextButton();
                if (i != (messages.Length - 1)) //ถ้าไม่ใช่ข้อความสุดท้ายของ Dialog ไม่ต้อง showChoice
                {
                    showChoice = false;
                }

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
                    AssignNextButton(dialog.Choices[0].Next, dialog.GetQuestId());

                }

                nextButton.SetActive(true); //แสดงปุ่ม Next 
                waitForSelect = true;

                if (showChoice)
                {
                    for (int j = 0; j < dialog.Choices.Count; j++)
                    {
                        if (j == 0)
                        {
                            AddQuizChoice(dialog.Choices[j].Message, dialog.Choices[j].Next, colorDots[j]);

                        }
                        else if (j == 1 )
                        {
                            AddQuizChoice(dialog.Choices[j].Message, dialog.Choices[j].Next, colorDots[j]);
                        }
                        else
                        {
                            AddQuizChoice(dialog.Choices[j].Message, dialog.Choices[j].Next, colorDots[j]);
                        }


                    }

                    nextButton.SetActive(false);
                    yield return new WaitUntil(() => !waitForSelect);
                }

                yield return null;
                yield return new WaitUntil(() => !nextButton.activeInHierarchy);
            }

            HideDialog();
            ClearDialog();


        }

        private void AddQuizChoice(string text, string nextId, Color buttonColor, string questId = null)
        {
            var choiceBut = Instantiate(choiceDotPrefab, choiceBox.transform);
            choiceGameobject.Add(choiceBut);

            var choiceButtonScript = choiceBut.GetComponent<ChoiceQuizButton>();

            choiceButtonScript.ChoiceText.text = text;
            choiceButtonScript.Dot.color = buttonColor;

        }


        private void AssignNextButton(string nextId = "", string questId = "", IQuestRunner questRunner = null)
        {
            var choiceButtonScript = nextButton.GetComponent<ChoiceButtonScript>();
            choiceButtonScript.NextId = nextId;
            choiceButtonScript.QuestId = questId;
            choiceButtonScript.QuestRunner = questRunner;
            choiceButtonScript.NpcDialogController = null;
        }

        private void AddDialog(Dialog dialog)
        {
            var dialogButton = Instantiate(choiceButtonPrefab, dialogBox.transform);
            dialogButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dialog.Name;
            var choiceButtonScript = dialogButton.GetComponent<ChoiceButtonScript>();
            PassDialogData(choiceButtonScript, dialog.NodeID, dialog.GetQuestId());

        }

        private void PassDialogData(ChoiceButtonScript choiceButtonScript, string nextId, string questId, IQuestRunner questRunner = null)
        {
            choiceButtonScript.NextId = nextId;
            choiceButtonScript.QuestId = questId;
            choiceButtonScript.QuestRunner = questRunner;
            choiceButtonScript.NpcDialogController = null;
        }
# endif
    }

}
