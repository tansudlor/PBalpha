using com.playbux.functioncollection;
using com.playbux.quest;
using com.playbux.sfxwrapper;
using com.playbux.tool;
using com.playbux.ui;
using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.networkquest
{
    public class ChoiceButtonScript : MonoBehaviour
    {
#if !SERVER
        [SerializeField]
        private TextMeshProUGUI[] choiceTexts;

        private string questId;
        private string nextId;
        private Dialog dialog;
        private DialogLinkOutData dialogLinkOutData;
        private IQuestRunner questRunner;
        private NPCDialogController npcDialogController;
        public string QuestId { get => questId; set => questId = value; }
        public string NextId { get => nextId; set => nextId = value; }
        public IQuestRunner QuestRunner { get => questRunner; set => questRunner = value; }
        public NPCDialogController NpcDialogController { get => npcDialogController; set => npcDialogController = value; }
        public TextMeshProUGUI[] ChoiceTexts { get => choiceTexts; set => choiceTexts = value; }
        public Dialog Dialog { get => dialog; set => dialog = value; }
        public DialogLinkOutData DialogLinkOutData { get => dialogLinkOutData; set => dialogLinkOutData = value; }

        public void OnClick()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Next");
            SpecialCall(dialog);
            this.gameObject.GetComponent<Button>().enabled = false;
            Debug.Log(questId + " x " + nextId);
            Debug.Log("aasd" + NpcDialogController);
            NpcDialogController.WaitForSelect = false;

            if (string.IsNullOrEmpty(nextId))
            {
                NpcDialogController.HideDialog();
                return;
            }

            QuestRunner.GetNext(nextId, questId);
        }

        public void OnClickNext()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Next");
            SpecialCall(dialog);

            if (dialogLinkOutData != null)
            {
                LinkOutCall(dialogLinkOutData);
            }

            if (!string.IsNullOrEmpty(nextId) && !string.IsNullOrEmpty(questId))
            {
                QuestRunner.GetNext(nextId, questId);
            }

            this.gameObject.SetActive(false);
        }

        private void SpecialCall(Dialog dialog)
        {
            try
            {
                Debug.Log("called " + JsonConvert.SerializeObject(dialog));

                var called = Dialog.Name.Split("=>")[1];
                Debug.Log("called " + called);
                QuestRunner.FunctionCall[called]?.Invoke();
            }
            catch
            {

            }
        }

        private void LinkOutCall(DialogLinkOutData dialogLinkOutData)
        {
            NpcDialogController.CallLinkOutSignalbus(dialogLinkOutData);
        }

#endif
    }

}
