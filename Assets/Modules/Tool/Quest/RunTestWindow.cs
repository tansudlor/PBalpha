#if UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.playbux.tool
{

    public class RunTestWindow : EditorWindow
    {
        private QuestGraphView graphView;
        private List<Dialog> node;
        private VisualElement buttonGroup = null;
        private Label labelQuestname = null;
        private Label labelQuestMessage = null;
        public static List<Dialog> questNodes;
        public static Dialog dialog;
        [MenuItem("Graph/Run")]
        public static void OpenWindow()
        {

            var window = GetWindow<RunTestWindow>("Run");
            window.minSize = new Vector2(800, 600);

        }

        public static void InputParameter(Dialog dialogInput)
        {

            dialog = dialogInput;
        }


        private void OnEnable()
        {

            Debug.Log(dialog);
            ShowDialog(dialog);
        }


        private void ShowDialog(Dialog dialog)
        {
            if (buttonGroup != null)
            {
                rootVisualElement.Remove(buttonGroup);
                rootVisualElement.Remove(labelQuestname);
                rootVisualElement.Remove(labelQuestMessage);
                buttonGroup = null;
                labelQuestname = null;
                labelQuestMessage = null;
            }
            buttonGroup = new VisualElement();
            labelQuestname = new Label();
            labelQuestMessage = new Label();




            buttonGroup.style.position = Position.Absolute;
            buttonGroup.style.alignSelf = Align.Center;
            buttonGroup.style.bottom = 0;
            buttonGroup.style.backgroundColor = new Color(1, 1, 1, 0.5f);
            buttonGroup.style.width = Length.Percent(100);
            buttonGroup.style.height = Length.Percent(50);

            if (dialog.Choices.Count > 0)
            {

                for (int i = 0; i < dialog.Choices.Count; i++)
                {
                    Debug.Log("ccccc" + i);
                    Debug.Log(dialog.Choices[i].Next);
                    var next = dialog.Choices[i].Next;
                    var choice = new Button() { text = dialog.Choices[i].Message };
                    buttonGroup.Add(choice);
                }
            }

            labelQuestname.style.position = Position.Relative;
            labelQuestname.style.top = Length.Percent(10);
            labelQuestname.style.fontSize = 30;
            labelQuestname.text = dialog.Name;
            labelQuestname.style.alignSelf = Align.Center;

            labelQuestMessage.style.position = Position.Relative;
            labelQuestMessage.style.top = Length.Percent(20);
            labelQuestMessage.style.fontSize = 20;
            labelQuestMessage.text = dialog.Message;
            labelQuestMessage.style.alignSelf = Align.Center;

            rootVisualElement.Add(labelQuestname);
            rootVisualElement.Add(labelQuestMessage);
            this.rootVisualElement.Add(buttonGroup);
        }

        private void ToNextLine(string nextNode)
        {

            if (nextNode == "")
            {
                Debug.Log("No next Node");
                return;
            }

            for (int i = 0; i < node.Count; i++)
            {
                if (node[i].NodeID == nextNode)
                {
                    Debug.Log("nodeID" + i);
                    //ShowDialog(i);
                    break;
                }

            }


        }
    }

}
#endif