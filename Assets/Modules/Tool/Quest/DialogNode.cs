#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.playbux.tool
{
    public class DialogNode : BaseNode
    {

        public TextField messageField;

        // public List<Choice> Choices { get; set; }

        private ListView listViewAddFlag;
        public List<string> itemListAddFlag;

        private ListView listViewRemoveFlag;
        public List<string> itemListRemoveFlag;

        private GraphView graphView;
        public static int adjustCounter = 0;
        private Dictionary<string, DialogNode> nodes;
        public Dialog Dialog;
        public Toggle finishQuestToggle;

        public ListView ListViewRemoveFlag { get => listViewRemoveFlag; set => listViewRemoveFlag = value; }
        public ListView ListViewAddFlag { get => listViewAddFlag; set => listViewAddFlag = value; }

        public DialogNode(GraphView graphView, Dictionary<string, DialogNode> nodes, Dialog dialog = null)
        {
            //  this.Choices = new List<Choice>();
            this.nodes = nodes;
            this.graphView = graphView;
            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Link";
            inputContainer.Add(inputPort);

            var EditButton = new Button(() => EditName()) { text = "E" };
            titleContainer.Add(EditButton);
            this.RefreshPorts();
            this.RefreshExpandedState();

            messageField = new TextField();
            messageField.multiline = true;
            // ตั้งค่าความกว้างและ word wrap
            messageField.style.width = 100;
            messageField.style.whiteSpace = WhiteSpace.Normal; // เปิดใช้งาน word wrap
            messageField.RegisterValueChangedCallback(evt =>
            {
                Dialog.Message = evt.newValue;
            });
            inputContainer.Add(messageField);

            finishQuestToggle = new Toggle("End Quest");
            finishQuestToggle.RegisterValueChangedCallback(evt =>
            {

                Dialog.EndQuest = evt.newValue;

            });
            inputContainer.Add(finishQuestToggle);

            var addButton = new Button(() => AddOutputPort()) { text = "Add Choice" };
            outputContainer.Add(addButton);
            this.RefreshPorts();
            this.RefreshExpandedState();


            extensionContainer.style.flexDirection = FlexDirection.Column; // Set the layout direction to row
            extensionContainer.style.alignItems = Align.Auto;
            extensionContainer.style.backgroundColor = Color.black;

            var addFlagBox = new VisualElement();
            string addFlagName = "Add Flag";
            var addflagBoxLabel = new Label();
            addflagBoxLabel.text = addFlagName;

            addFlagBox.style.flexDirection = FlexDirection.Row;
            addFlagBox.style.alignItems = Align.Center;
            addFlagBox.Add(addflagBoxLabel);

            ListViewAddFlag = new ListView();
            itemListAddFlag = new List<string>();

            ListViewAddFlag.RegisterCallback<PointerUpEvent>(evt =>
            {

                if (QuestGraphViewWindow.flagTemp != "")
                {
                    itemListAddFlag.Add(QuestGraphViewWindow.flagTemp);
                    ListViewAddFlag.Rebuild();
                }
                QuestGraphViewWindow.flagTemp = "";
            });
            ListViewAddFlag.itemsSource = itemListAddFlag;
            ListViewAddFlag.makeItem = CreateLabel;
            ListViewAddFlag.bindItem = (element, i) => (element as Label).text = ListViewAddFlag.itemsSource[i].ToString();
            ListViewAddFlag.style.height = 50;
            ListViewAddFlag.style.width = 100;
            ListViewAddFlag.style.backgroundColor = Color.gray;
            ListViewAddFlag.style.alignContent = Align.FlexStart;
            ListViewAddFlag.itemsChosen += RemoveAddFlagChosen;

            addFlagBox.Add(ListViewAddFlag);

            var removeFlagBox = new VisualElement();
            string removeFlagName = "Remove Flag";
            var removeflagBoxLabel = new Label();
            removeflagBoxLabel.text = removeFlagName;

            removeFlagBox.style.flexDirection = FlexDirection.Row;
            removeFlagBox.style.alignItems = Align.Center;
            removeFlagBox.Add(removeflagBoxLabel);

            ListViewRemoveFlag = new ListView();
            itemListRemoveFlag = new List<string>();

            ListViewRemoveFlag.RegisterCallback<PointerUpEvent>(evt =>
            {

                if (QuestGraphViewWindow.flagTemp != "")
                {
                    itemListRemoveFlag.Add(QuestGraphViewWindow.flagTemp);
                    ListViewRemoveFlag.Rebuild();
                }
                QuestGraphViewWindow.flagTemp = "";
            });
            ListViewRemoveFlag.itemsSource = itemListRemoveFlag;
            ListViewRemoveFlag.makeItem = CreateLabel;
            ListViewRemoveFlag.bindItem = (element, i) => (element as Label).text = ListViewRemoveFlag.itemsSource[i].ToString();
            ListViewRemoveFlag.style.height = 50;
            ListViewRemoveFlag.style.width = 100;
            ListViewRemoveFlag.style.backgroundColor = Color.gray;
            ListViewRemoveFlag.style.alignContent = Align.FlexStart;
            ListViewRemoveFlag.itemsChosen += RemoveRemoveFlagChosen;

            removeFlagBox.Add(ListViewRemoveFlag);

            extensionContainer.Add(addFlagBox);
            extensionContainer.Add(removeFlagBox);
            this.RefreshPorts();
            this.RefreshExpandedState();
            if (dialog == null)
            {
                Dialog = new Dialog(this);
            }
            else
            {
                Dialog = dialog;
                dialog.Apply(this, "dialogNode");
            }
        }

        public void UpdateStatus()
        {
            if (Dialog.Position == null)
            {
                Dialog.Position = "0,0";
            }

            if (Dialog.Position.IndexOf(',') < 0)
            {
                Dialog.Position = (200 * (adjustCounter % 3)) + "," + (100 * (adjustCounter / 3) + 100);
                adjustCounter++;
            }

            var spl = Dialog.Position.Split(',');
            SetPosition(new Rect(float.Parse(spl[0]), float.Parse(spl[1]), 500, 500));
            if (Dialog.Choices == null)
            {
                Dialog.Choices = new List<Choice>();
            }
            for (int i = Dialog.Choices.Count - 1; i >= 0; i--)
            {
                if (Dialog.Choices[i].Message == "[DELETED]")
                {
                    Dialog.Choices.RemoveAt(i);
                }
            }


            for (int i = 0; i < Dialog.Choices.Count; i++)
            {
                var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                DetailOutputPort(Dialog.Choices[i].Message, outputPort);
                if (Dialog.Choices == null)
                    continue;
                if (Dialog.Choices[i] == null)
                    continue;
                if (Dialog.Choices[i].Next == null)
                    continue;
                if (!nodes.ContainsKey(Dialog.Choices[i].Next))
                    continue;
                Edge edge = outputPort.ConnectTo(nodes[Dialog.Choices[i].Next].inputContainer[0] as Port);
                // Add edge to the graph
                graphView.AddElement(edge);
            }
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
            this.tooltip = Dialog.NodeID;
        }



        public List<string> AddToList(string allFlag)
        {
            return allFlag.Split(new String[] { "■■" }, StringSplitOptions.None).ToList();
        }

        public string AddToString(List<string> listFlag)
        {
            return string.Join("■■", listFlag);
        }

        private void RemoveAddFlagChosen(IEnumerable<object> enumerable)
        {
            if (enumerable != null && enumerable.Any())
            {
                int selectedIndex = itemListAddFlag.IndexOf(enumerable.First().ToString());
                if (selectedIndex >= 0)
                {
                    itemListAddFlag.RemoveAt(selectedIndex);
                    ListViewAddFlag.Rebuild();
                }

            }
        }

        private void RemoveRemoveFlagChosen(IEnumerable<object> enumerable)
        {
            if (enumerable != null && enumerable.Any())
            {
                int selectedIndex = itemListRemoveFlag.IndexOf(enumerable.First().ToString());
                if (selectedIndex >= 0)
                {
                    itemListRemoveFlag.RemoveAt(selectedIndex);
                    ListViewRemoveFlag.Rebuild();
                }

            }
        }

        Label CreateLabel()
        {
            var label = new Label();
            return label;
        }

        private TextField FlagCreate(VisualElement boxMsg, string label, TextField field, string flag)
        {

            var flagBox = new Label();
            flagBox.text = label;
            boxMsg.style.flexDirection = FlexDirection.Row;
            boxMsg.style.alignItems = Align.Center;

            field = new TextField();
            field.style.width = 100;
            field.style.whiteSpace = WhiteSpace.Normal;

            field.RegisterValueChangedCallback(evt =>
            {
                this.GetType().GetProperty(flag).SetValue(this, evt.newValue);
            });

            boxMsg.Add(flagBox);
            boxMsg.Add(field);
            return field;

        }

        private void EditName()
        {
            TextInputDialog.OpenDialog((inputtext) =>
            {
                Dialog.Name = inputtext;
                SetTitle(Dialog.Name);
            }, Dialog.Name);
        }

        private void DetailOutputPort(string inputtext, Port outputPort)
        {

            if (inputtext.Length >= 10)
            {
                outputPort.portName = inputtext.Substring(0, 10).Replace('\n', ' ');
            }
            else
            {
                outputPort.portName = inputtext.Replace('\n', ' ');
            }
            // สร้าง VisualElement สำหรับจัดวางปุ่ม
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row; // ปุ่มเรียงกันแนวนอน

            // เพิ่มปุ่มลบ
            var deleteButton = new Button(() => RemovePort(outputPort)) { text = "X" };
            buttonsContainer.Add(deleteButton);

            // เพิ่มปุ่มแก้ไข
            var editButton = new Button(() => EditPort(outputPort)) { text = "E" };
            buttonsContainer.Add(editButton);

            // จัดวาง buttonsContainer ใน contentContainer ของ port
            outputPort.contentContainer.Add(buttonsContainer);
            outputPort.contentContainer.Add(outputPort.Q<Label>("type")); // ถ้ามี label, ให้เพิ่มหลัง buttonsContainer

            outputContainer.Add(outputPort);
            this.RefreshPorts();
        }

        public override Port AddOutputPort(string inputtext = "")
        {
            Port outputPort = null;
            if (inputtext == "")
            {
                TextInputDialog.OpenDialog((inputtext) =>
                {
                    Debug.Log("haaaaa");
                    outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                    DetailOutputPort(inputtext, outputPort);
                    for (int i = 0; i < outputContainer.childCount; i++)
                    {
                        Debug.Log("Naaaa");
                        if (outputContainer[i] == outputPort)
                        {
                            DialogNode node = this;
                            if (node.Dialog.Choices.Count < i)
                            {
                                node.Dialog.Choices.Add(new Choice());
                            }
                            node.Dialog.Choices[i - 1].Message = inputtext;
                            break;
                        }
                    }
                });
            }
            else
            {
                outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                DetailOutputPort(inputtext, outputPort);
            }
            return outputPort;
        }
        public override Port ApplyPort(string inputText, int portIndex)
        {
            Port outputPort = base.ApplyPort(inputText, portIndex);
            for (int i = 0; i < outputContainer.childCount; i++)
            {
                if (outputContainer[i] == outputPort)
                {
                    DialogNode node = this;
                    if (node.Dialog.Choices.Count < i)
                    {
                        node.Dialog.Choices.Add(new Choice());
                    }
                    node.Dialog.Choices[i - 1].Message = inputText;
                    break;
                }
            }
            return outputPort;
        }

        private void EditPort(Port outputPort)
        {
            TextInputDialog.OpenDialog((inputtext) =>
            {
                if (inputtext.Length >= 10)
                {
                    outputPort.portName = inputtext.Substring(0, 10).Replace('\n', ' ');
                }
                else
                {
                    outputPort.portName = inputtext.Replace('\n', ' ');
                }
                for (int i = 0; i < outputContainer.childCount; i++)
                {
                    if (outputContainer[i] == outputPort)
                    {
                        DialogNode node = this;
                        if (node.Dialog.Choices.Count < i)
                        {
                            node.Dialog.Choices.Add(new Choice());
                        }
                        node.Dialog.Choices[i - 1].Message = inputtext;
                        break;
                    }
                }
            }, outputPort.tooltip);
        }

        private void EditPort(Port outputPort, string inputtext)
        {
            if (inputtext.Length >= 10)
            {
                outputPort.portName = inputtext.Substring(0, 10).Replace('\n', ' ');
            }
            else
            {
                outputPort.portName = inputtext.Replace('\n', ' ');
            }
            for (int i = 0; i < outputContainer.childCount; i++)
            {
                if (outputContainer[i] == outputPort)
                {
                    DialogNode node = this;
                    if (node.Dialog.Choices.Count < i)
                    {
                        node.Dialog.Choices.Add(new Choice());
                    }
                    node.Dialog.Choices[i - 1].Message = inputtext;
                    break;
                }
            }
        }


        private void RemovePort(Port port)
        {
            EditPort(port, "[DELETED]");
            //Choice choice = QuestGraphViewWindow.GetChoice(port);
            //choice.message = "[DELETED]";
            //choice.next = null;

            var edgesToRemove = new List<Edge>();
            edgesToRemove.AddRange(port.connections);

            foreach (var edge in edgesToRemove)
            {
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            //outputContainer.Remove(port); // ลบพอร์ตจาก output container
        }



    }

}
#endif