#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.playbux.tool
{
    public class StartNodeList
    {

        private VisualElement container;

        public StartNodeList(VisualElement container)
        {
            this.container = container;
        }

        public string this[int index]
        {
            get
            {
                
                if (index > container.childCount)
                {
                    return "";
                }
                return container.Children().ElementAt(index + 1).tooltip;
                // return null;
            }
            set
            {
                container.Children().ElementAt(index + 1).tooltip = value;
            }
        }

        public List<string> ToList()
        {
            var ret = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                ret.Add(this[i]);
            }
            return ret;
        }

        public void FromList(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                this[i] = list[i];
            }
        }
        public int Count
        {
            get
            {
                return container.childCount -1 ;
            }
        }
    }

    public class NonPlayerCharacterNode : BaseNode
    {
        public NonPlayerCharacter NonPlayerCharacter;
        private GraphView graphView;
        private Dictionary<string, NonPlayerCharacterNode> nodes;

        public TextField nonPlayerCharacterBoxField;
        public static int adjustCounterNonPlayerNode = 0;
        public NonPlayerCharacterNode(GraphView graphView, Dictionary<string, NonPlayerCharacterNode> node, NonPlayerCharacter npc = null)
        {

            this.graphView = graphView;
            this.nodes = node;
            var EditButton = new Button(() => EditName()) { text = "E" };
            titleContainer.Add(EditButton);
            this.RefreshPorts();
            this.RefreshExpandedState();

            var nonPlayerCharacterBox = new VisualElement();
            nonPlayerCharacterBoxField = new TextField();
            string nonPlayerCharacterText = "NPC ID";

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Link";
            inputContainer.Add(inputPort);

            var flagBox = new Label();
            flagBox.text = nonPlayerCharacterText;
            nonPlayerCharacterBox.style.flexDirection = FlexDirection.Row;
            nonPlayerCharacterBox.style.alignItems = Align.Center;

            nonPlayerCharacterBoxField.style.width = 100;
            nonPlayerCharacterBoxField.style.whiteSpace = WhiteSpace.Normal;

            nonPlayerCharacterBox.Add(flagBox);
            nonPlayerCharacterBox.Add(nonPlayerCharacterBoxField);


            inputContainer.style.alignItems = Align.FlexStart;
            inputContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            inputContainer.Add(nonPlayerCharacterBox);
            this.RefreshPorts();
            this.RefreshExpandedState();

            var addButton = new Button(() => AddOutputPort()) { text = "Start Dialog" };
            outputContainer.Add(addButton);
            this.RefreshPorts();
            this.RefreshExpandedState();
            if (npc == null)
            {
                NonPlayerCharacter = new NonPlayerCharacter(this, new StartNodeList(outputContainer));
            }
            else
            {
                NonPlayerCharacter = npc;
                NonPlayerCharacter.Apply(this, new StartNodeList(outputContainer));
            }
        }

        private void EditName()
        {
            TextInputDialog.OpenDialog((inputtext) =>
            {
                NonPlayerCharacter.Name = inputtext;
                SetTitle(NonPlayerCharacter.Name);
            }, NonPlayerCharacter.Name);
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
            this.tooltip = NonPlayerCharacter.NodeID;
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
                outputPort.tooltip = inputtext;
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
            outputPort.tooltip = inputtext;
            for (int i = 0; i < outputContainer.childCount; i++)
            {
                /*  if (outputContainer[i] == outputPort)
                  {
                      QuestNode node = this;
                      if (node.Choices.Count < i)
                      {
                          node.Choices.Add(new Choice());
                      }
                      node.Choices[i - 1].Message = inputtext;
                      break;
                  }*/
            }
        }

        

        public override Port AddOutputPort(string inputtext = "")
        {
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            DetailOutputPort(inputtext, outputPort);
            outputPort.tooltip = inputtext;
            return outputPort;
        }

        private void DetailOutputPort(string inputtext, Port outputPort)
        {
            outputPort.tooltip = inputtext;
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

            // จัดวาง buttonsContainer ใน contentContainer ของ port
            outputPort.contentContainer.Add(buttonsContainer);
            outputPort.contentContainer.Add(outputPort.Q<Label>("type")); // ถ้ามี label, ให้เพิ่มหลัง buttonsContainer

            outputContainer.Add(outputPort);
            this.RefreshPorts();


        }



        public void UpdateStatus()
        {
            if (NonPlayerCharacter.Position == null)
            {
                NonPlayerCharacter.Position = "0,0";
            }

            if (NonPlayerCharacter.Position.IndexOf(',') < 0)
            {
                NonPlayerCharacter.Position = (200 * (adjustCounterNonPlayerNode % 3)) + "," + (100 * (adjustCounterNonPlayerNode / 3) + 100);
                adjustCounterNonPlayerNode++;
            }

            var spl = NonPlayerCharacter.Position.Split(',');
            SetPosition(new Rect(float.Parse(spl[0]), float.Parse(spl[1]), 500, 500));
        }


    }
}
#endif

