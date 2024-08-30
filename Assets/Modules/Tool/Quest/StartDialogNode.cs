#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.playbux.tool
{
    public class StartDialogNode : BaseNode
    {
        public StartDialog StartDialog;

        public static int adjustCounterNonPlayerNode = 0;

        private GraphView graphView;
        private Dictionary<string, StartDialogNode> nodes;

        private ListView acceptListView;
        public List<string> acceptFlagList;

        private ListView rejectListView;
        public List<string> rejectFlagList;

        public ListView AcceptListView { get => acceptListView; set => acceptListView = value; }
        public ListView RejectListView { get => rejectListView; set => rejectListView = value; }

        public StartDialogNode(GraphView graphView, Dictionary<string, StartDialogNode> node, StartDialog startDialog = null)
        {

            this.graphView = graphView;
            //startDialogNodes = new StartNodeList(outputContainer);
            this.nodes = node;
            var EditButton = new Button(() => EditName()) { text = "E" };
            titleContainer.Add(EditButton);
            this.RefreshPorts();
            this.RefreshExpandedState();

            var StartDialogBox = new VisualElement();

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Link";
            inputContainer.Add(inputPort);

            var startBox = new Label();

            StartDialogBox.style.flexDirection = FlexDirection.Row;
            StartDialogBox.style.alignItems = Align.Center;

            inputContainer.style.alignItems = Align.FlexStart;
            inputContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            inputContainer.Add(StartDialogBox);

            var outputToQuestPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));

            outputToQuestPort.portName = "Link Start Quest";

            outputContainer.Add(outputToQuestPort);

            var acceptFlagBox = new VisualElement();
            var removeFlagBox = new VisualElement();

            string acceptFlagName = "Accept Flag";
            string removeFlagName = "Reject Flag";

            var flagAcceptBox = new Label();
            var flagRemoveBox = new Label();
            flagAcceptBox.text = acceptFlagName;
            flagRemoveBox.text = removeFlagName;


            acceptFlagBox.style.flexDirection = FlexDirection.Row;
            acceptFlagBox.style.alignItems = Align.Center;
            acceptFlagBox.Add(flagAcceptBox);

            removeFlagBox.style.flexDirection = FlexDirection.Row;
            removeFlagBox.style.alignItems = Align.Center;
            removeFlagBox.Add(flagRemoveBox);

            AcceptListView = new ListView();
            acceptFlagList = new List<string>();

            AcceptListView.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (QuestGraphViewWindow.flagTemp != "")
                {
                    acceptFlagList.Add(QuestGraphViewWindow.flagTemp);
                    AcceptListView.Rebuild();
                }
                QuestGraphViewWindow.flagTemp = "";
            });
            AcceptListView.itemsSource = acceptFlagList;
            AcceptListView.makeItem = CreateLabel;
            AcceptListView.bindItem = (element, i) => (element as Label).text = AcceptListView.itemsSource[i].ToString();
            AcceptListView.style.height = 50;
            AcceptListView.style.width = 100;
            AcceptListView.style.backgroundColor = Color.black;
            AcceptListView.style.alignContent = Align.FlexStart;
            AcceptListView.itemsChosen += RemoveFlagChosenAccept;

            acceptFlagBox.Add(AcceptListView);

            RejectListView = new ListView();
            rejectFlagList = new List<string>();
            RejectListView.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (QuestGraphViewWindow.flagTemp != "")
                {
                    rejectFlagList.Add(QuestGraphViewWindow.flagTemp);
                    RejectListView.Rebuild();

                }
                QuestGraphViewWindow.flagTemp = "";
            });
            RejectListView.itemsSource = rejectFlagList;
            RejectListView.makeItem = CreateLabel;
            RejectListView.bindItem = (element, i) => (element as Label).text = RejectListView.itemsSource[i].ToString();
            RejectListView.style.height = 50;
            RejectListView.style.width = 100;
            RejectListView.style.backgroundColor = Color.black;
            RejectListView.style.alignContent = Align.FlexStart;
            RejectListView.itemsChosen += RemoveFlagChosenRemove;

            removeFlagBox.Add(RejectListView);


            extensionContainer.style.flexDirection = FlexDirection.Row; // Set the layout direction to row
            extensionContainer.style.alignItems = Align.Auto;
            extensionContainer.style.backgroundColor = Color.black;
            extensionContainer.Add(acceptFlagBox);
            extensionContainer.Add(removeFlagBox);
            this.RefreshPorts();
            this.RefreshExpandedState();

            if (startDialog == null)
            {
                StartDialog = new StartDialog(this);
            }
            else
            {
                StartDialog = startDialog;
                startDialog.Apply(this, "startDialogNode");
            }
        }

        public List<string> AddToList(string allFlag)
        {
            return allFlag.Split(new String[] { "■■" }, StringSplitOptions.None).ToList();
        }

        public string AddToString(List<string> listFlag)
        {
            return string.Join("■■", listFlag);
        }


        private void RemoveFlagChosenRemove(IEnumerable<object> enumerable)
        {
            
            if (enumerable != null && enumerable.Any())
            {
                int selectedIndex = rejectFlagList.IndexOf(enumerable.First().ToString());
                if (selectedIndex >= 0)
                {
                    rejectFlagList.RemoveAt(selectedIndex);
                    RejectListView.Rebuild();
                }

            }
        }
        public override Port AddOutputPort(string inputtext = "")
        {
            return null;
        }
        private void RemoveFlagChosenAccept(IEnumerable<object> enumerable)
        {
            if (enumerable != null && enumerable.Any())
            {
                int selectedIndex = acceptFlagList.IndexOf(enumerable.First().ToString());
                if (selectedIndex >= 0)
                {
                    acceptFlagList.RemoveAt(selectedIndex);

                    AcceptListView.Rebuild();
                }

            }
        }
        Label CreateLabel()
        {
            var label = new Label();
            return label;
        }
        private void EditName()
        {
            TextInputDialog.OpenDialog((inputtext) =>
            {
                StartDialog.Name = inputtext;
                SetTitle(StartDialog.Name);
            }, StartDialog.Name);
        }
        public void SetTitle(string inputtext)
        {
            if (inputtext == null)
            {
                inputtext = "Untitled";
            }
            if (inputtext.Length >= 10)
            {
                title = inputtext.Substring(0, 10).Replace('\n', ' ') + "..";
            }
            else
            {
                title = inputtext.Replace('\n', ' ');
            }
            this.tooltip = StartDialog.NodeID;
        }
    }
}
#endif

