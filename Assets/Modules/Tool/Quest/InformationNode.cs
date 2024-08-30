#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace com.playbux.tool
{
    public class NonPlayerNodeList
    {

        private VisualElement container;

        public NonPlayerNodeList(VisualElement container)
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

        public int Count { get => container.childCount - 1; }
    }
    public class InformationNode : BaseNode
    {
        public Information Information;
        private GraphView graphView;
        private Dictionary<string, InformationNode> nodes;
        private TextField questNametextField;
        private TextField questIdtextField;
        private string allFlag;
        private string selectFlag;
        public static int adjustCounterQuestNodeInformation = 0;
        public static Label FlagNameLabel;


        public TextField QuestNametextField
        {
            get => questNametextField;

        }
        public TextField QuestIdtextField
        {
            get => questIdtextField;

        }
        public NonPlayerNodeList NonPlayerNodes { get => nonPlayerNodes; set => nonPlayerNodes = value; }


        private NonPlayerNodeList nonPlayerNodes;



        // Start is called before the first frame update
        public InformationNode(GraphView graphView,Information info = null)
        {
            
            this.graphView = graphView;
            nonPlayerNodes = new NonPlayerNodeList(outputContainer);
            var questName = new VisualElement();
            questNametextField = new TextField();
            string questNameText = "Quest Name";

            var questNameElement = Boxmsg(questName, questNameText, QuestNametextField);

            var questId = new VisualElement();
            questIdtextField = new TextField();
            string questIdText = "Quest ID";

            var questIdElement = Boxmsg(questId, questIdText, QuestIdtextField);

            inputContainer.style.alignItems = Align.Auto;
            inputContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            inputContainer.Add(questNameElement);
            inputContainer.Add(questIdElement);
            
            var addButton = new Button(() => AddOutputPort()) { text = "NPC" };
            outputContainer.Add(addButton);
            this.RefreshPorts();
            this.RefreshExpandedState();
            if (info == null)
            {
                Information = new Information(this);
            }
            else
            {
                Information = info;
                Information.Apply(this, "informationNode");
            }
            this.title = "Information";
        }

        public override Port  AddOutputPort(string inputtext = "")
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

            // เพิ่มปุ่มแก้ไข
            /*var editButton = new Button(() => EditPort(outputPort)) { text = "E" };
            buttonsContainer.Add(editButton);*/

            // จัดวาง buttonsContainer ใน contentContainer ของ port
            outputPort.contentContainer.Add(buttonsContainer);
            outputPort.contentContainer.Add(outputPort.Q<Label>("type")); // ถ้ามี label, ให้เพิ่มหลัง buttonsContainer

            outputContainer.Add(outputPort);
            this.RefreshPorts();
           
        }

        public static VisualElement Boxmsg(VisualElement boxMsg, string label, TextField field)
        {

            var flagBox = new Label();
            flagBox.text = label;
            boxMsg.style.flexDirection = FlexDirection.Row;
            boxMsg.style.alignItems = Align.Center;

            field.style.width = 100;
            field.style.whiteSpace = WhiteSpace.Normal;

            boxMsg.Add(flagBox);
            boxMsg.Add(field);

            return boxMsg;
        }



        private void RemovePort(Port port)
        {
            EditPort(port, "[DELETED]");

            var edgesToRemove = new List<Edge>();
            edgesToRemove.AddRange(port.connections);

            foreach (var edge in edgesToRemove)
            {
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

        }


        public  void EditPort(Port outputPort, string inputtext)

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

        public void UpdateStatus()
        {
            if (Information.Position == null)
            {
                Information.Position = "0,0";
            }

            if (Information.Position.IndexOf(',') < 0)
            {
                Information.Position = (200 * (adjustCounterQuestNodeInformation % 3)) + "," + (100 * (adjustCounterQuestNodeInformation / 3) + 100);
                adjustCounterQuestNodeInformation++;
            }

            var spl = Information.Position.Split(',');
            SetPosition(new Rect(float.Parse(spl[0]), float.Parse(spl[1]), 500, 500));
        }
    }
}
#endif