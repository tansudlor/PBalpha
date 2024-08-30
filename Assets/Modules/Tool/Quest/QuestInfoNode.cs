#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    public class QuestInfoNode : BaseNode
    {
        private GraphView graphView;
        public QuestInformation QuestInformation;
        private Dictionary<string, QuestInfoNode> nodes;
        public QuestInfoNode(GraphView graphView, Dictionary<string, QuestInfoNode> node, QuestInformation questInformation = null)
        {
            this.nodes = node;
            this.graphView = graphView;
            var EditButton = new Button(() => EditName()) { text = "E" };                  
            titleContainer.Add(EditButton);

            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Link";
            inputContainer.Add(inputPort);

            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputContainer.Add(outputPort);


            extensionContainer.style.flexDirection = FlexDirection.Column; // Set the layout direction to row
            extensionContainer.style.alignItems = Align.FlexStart;
            extensionContainer.style.backgroundColor = Color.black;

            var addDescriptionButton = new Button(() => AddDescription()) { text = "Add Description" };
            inputContainer.Add(addDescriptionButton);
            this.RefreshPorts();
            this.RefreshExpandedState();

            if (questInformation == null)
            {
                QuestInformation = new QuestInformation(this);
            }
            else
            {
                QuestInformation = questInformation;
                questInformation.Apply(this, "questInfoNode");
            }
        }

        private void DeleteDescription(VisualElement contaniner)
        {
            extensionContainer.Remove(contaniner);
        }

        public VisualElement AddDescription()
        {
            var descriptionContaniner = new VisualElement();
            var descriptionTextField = new TextField();
            var descriptionLabel = new Label();
            var descriptionActivteFlag = new ListView();
            var descriptionActivteFlagLabel = new Label();
            var descriptionFinishFlag = new ListView();
            var descriptionFinishFlagLabel = new Label();
            var deleteButton = new Button() { text = "X" };
            deleteButton.clicked += () =>
            {
                Debug.Log("delete");
                Debug.Log(deleteButton.parent.GetType());
                DeleteDescription(deleteButton.parent);
            };
            
            descriptionContaniner.style.flexDirection = FlexDirection.Row;
            descriptionContaniner.style.alignItems = Align.Center;
            descriptionActivteFlag.style.width = 75;
            descriptionActivteFlag.style.height = 75;
            descriptionActivteFlag.style.backgroundColor = Color.gray;
            descriptionActivteFlag.style.alignContent = Align.FlexStart;
            descriptionFinishFlag.style.width = 75;
            descriptionFinishFlag.style.height = 75;
            descriptionFinishFlag.style.backgroundColor = Color.gray;
            descriptionFinishFlag.style.alignContent = Align.FlexStart;
            descriptionTextField.style.width = 100;
            descriptionTextField.multiline = true;

            descriptionLabel.text = "Quest Description";
            descriptionActivteFlagLabel.text = "Activate Flag";
            descriptionFinishFlagLabel.text = "Finish Flag";
            
            descriptionActivteFlag.itemsSource = new List<string>();
            descriptionActivteFlag.RegisterCallback<PointerUpEvent>(evt =>
            {
               
                if (QuestGraphViewWindow.flagTemp != "")
                {
                    descriptionActivteFlag.itemsSource.Add(QuestGraphViewWindow.flagTemp);
                    descriptionActivteFlag.Rebuild();
                }
                QuestGraphViewWindow.flagTemp = "";
            });

            descriptionActivteFlag.makeItem = CreateLabel;
            descriptionActivteFlag.bindItem = (element, i) => (element as Label).text = descriptionActivteFlag.itemsSource[i].ToString();
           
            descriptionFinishFlag.itemsSource = new List<string>();
            
            descriptionFinishFlag.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (QuestGraphViewWindow.flagTemp != "")
                {
                    descriptionFinishFlag.itemsSource.Add(QuestGraphViewWindow.flagTemp);
                    descriptionFinishFlag.Rebuild();
                }
                QuestGraphViewWindow.flagTemp = "";
            });

            descriptionFinishFlag.makeItem = CreateLabel;
            descriptionFinishFlag.bindItem = (element, i) => (element as Label).text = descriptionFinishFlag.itemsSource[i].ToString();
            
            descriptionContaniner.Add(descriptionLabel);
            descriptionContaniner.Add(descriptionTextField);
            descriptionContaniner.Add(descriptionActivteFlagLabel);
            descriptionContaniner.Add(descriptionActivteFlag);
            descriptionContaniner.Add(descriptionFinishFlagLabel);
            descriptionContaniner.Add(descriptionFinishFlag);
            descriptionContaniner.Add(deleteButton);
            extensionContainer.Add(descriptionContaniner);
            this.RefreshPorts();
            this.RefreshExpandedState();
            return descriptionContaniner;
        }

        

        Label CreateLabel()
        {
            var label = new Label();
            label.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.clickCount == 2) 
                {
                    var listView = ((ListView)((Label)evt.target).parent.parent);
                    var list = listView.itemsSource;
                    var remove = ((Label)evt.target).text;
                    list.Remove(remove);
                    listView.Rebuild();

                }

            });
            return label;
        }

        private void EditName()
        {
            TextInputDialog.OpenDialog((inputtext) =>
            {
                QuestInformation.Name = inputtext;
                SetTitle(QuestInformation.Name);
            }, QuestInformation.Name);
        }

        public void SetTitle(string inputtext)
        {
            if (inputtext == null)
            {
                inputtext = "Untitled";
            }
            
            else
            {
                title = inputtext.Replace('\n', ' ');
            }
            this.tooltip = QuestInformation.NodeID;
        }

        public override Port AddOutputPort(string inputtext = "")
        {
            throw new System.NotImplementedException();
        }

        

    }


}
#endif