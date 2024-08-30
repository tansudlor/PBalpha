using com.playbux.input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace com.playbux.ui.interactdialog
{

    public class InteractDialog : MonoBehaviour, IInteractDialog
    {

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
        [SerializeField]
        private CanvasGroup canvasGroup;


        private Coroutine RunDialogCoroutine;
        private Coroutine RunSetTextCoroutine;


        void Start()
        {
            Hide();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                var dialog = new DefaultDialog("Hello");
                Show(dialog);
            }
        }

        public void Show(IDialog dialog)
        {
            ShowDialog();
            SetText(dialog.Text);
            //dialog.Process();
        }



        public void Hide()
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void ShowDialog()
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

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

        /*public void AssignNextButton(string nextId = "", string questId = "")
        {
            var choiceButtonScript = nextButton.GetComponent<ChoiceButtonScript>();
            choiceButtonScript.NextId = nextId;
            choiceButtonScript.QuestId = questId;
            choiceButtonScript.QuestRunner = questRunner;
            choiceButtonScript.NpcDialogController = this;
        }*/



    }
}
