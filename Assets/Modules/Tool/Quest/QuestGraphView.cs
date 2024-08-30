#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System;

namespace com.playbux.tool
{
    public class QuestGraphView : GraphView
    {
        public QuestGraphViewWindow window;
        private const float MinZoomFactor = 0.1f;
        private const float MaxZoomFactor = 2f;
        public static UnityEditor.Experimental.GraphView.Node informationNode;
        public static List<UnityEditor.Experimental.GraphView.Node> nonPlayerCharacterNode;
        public static int npcNodeCount = 0;
        public QuestGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.style.backgroundColor = new Color(0.1f, 0.3f, 0.3f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.SetupZoom(0.01f, 4f);
            this.graphViewChanged += OnGraphViewChanged;

        }




        public GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    /*  if (element is PositionNode node)
                      {
                          node.Position = element.GetPosition().x + "," + element.GetPosition().y;
                      }
                    */
                }
            }

            //ตอนย้ายเส้น
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        edge.output.portName = "";
                        edge.output.tooltip = "";

                        if (edge.output.node.GetType() == typeof(DialogNode))
                        {
                            DialogNode outputNode = edge.output.node as DialogNode;
                            DialogNode inputNode = edge.input.node as DialogNode;
                            Choice[] choices = outputNode.Dialog.Choices.Where(obj => obj.Message == edge.output.portName).ToArray();
                            if (choices.Length > 0)
                            {
                                choices[0].Next = "";
                            }
                            /*for (int i = 0; i < outputNode.outputContainer.childCount; i++)
                            {
                                if (outputNode.outputContainer[i].GetType() == typeof(Port))
                                {
                                    if (((Port)outputNode.outputContainer[i]).portName == edge.output.portName)
                                    {
                                        Debug.Log("Nullllll");
                                        outputNode.Dialog.Choices[i].Next = "";
                                    }
                                }

                            }*/
                            Dialog snode = new Dialog(outputNode);
                            string js = JsonConvert.SerializeObject(snode);
                            Debug.Log(js);
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    Debug.Log("create Edge");
                    if (edge.output.node.GetType() != typeof(DialogNode))
                    {
                        edge.output.portName = edge.input.node.title;
                        edge.output.tooltip = edge.input.node.tooltip;
                    }

                    if (edge.output.node.GetType() == typeof(DialogNode))
                    {
                        Debug.Log("create Edge in Dialog");
                        DialogNode outputNode = edge.output.node as DialogNode;
                        DialogNode inputNode = edge.input.node as DialogNode;
                        Debug.Log("sad" + edge.output.portName);
                        Choice[] choices = outputNode.Dialog.Choices.Where(obj => obj.Message == edge.output.portName).ToArray();

                        Debug.Log(choices.Length);
                        if (choices.Length > 0)
                        {
                            Debug.Log("Come");
                            choices[0].Next = edge.input.node.tooltip;
                        }
                        Dialog snode = new Dialog(outputNode);
                        string js = JsonConvert.SerializeObject(snode);
                        Debug.Log(js);
                        edge.output.tooltip = edge.input.node.tooltip;
                    }
                }
            }

            return graphViewChange;
        }



        public DialogNode CreateNode(string nodeName, Dictionary<string, DialogNode> nodes)
        {
            window.SaveNodeFile(true);
            var node = new DialogNode(this, nodes) { title = nodeName };
            node.Dialog.NodeID = Guid.NewGuid().ToString() + DateTime.Now.Ticks;
            node.SetTitle(nodeName);
            AddElement(node);
            return node;
        }

        public QuestInfoNode CreateQuestInfoNode (string nodename,Dictionary<string, QuestInfoNode> nodes)
        {
            window.SaveNodeFile(true);
            var node = new QuestInfoNode(this, nodes) { title = nodename };
            node.QuestInformation.NodeID = Guid.NewGuid().ToString() + DateTime.Now.Ticks;
            node.SetTitle(nodename);
            AddElement(node);
            return node;
        }

        public InformationNode NodeIndexCreate(string nodename)
        {
            
            var node = new InformationNode(this) { title = nodename };
            informationNode = node;
            AddElement(node);
            node.RegisterCallback<ContextualMenuPopulateEvent>(e => e.StopPropagation());

            // Disable the ability to delete with the delete key
            node.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    e.StopPropagation();
                }
            });
            return node;
        }

        public NonPlayerCharacterNode NonPlayerCharacterNodeCreate(string nodename, int count, Dictionary<string, NonPlayerCharacterNode> nodes)
        {
            window.SaveNodeFile(true);
            var node = new NonPlayerCharacterNode(this, nodes) { title = nodename };
            node.NonPlayerCharacter.NodeID = Guid.NewGuid().ToString() + DateTime.Now.Ticks;
            node.SetTitle("NPC" + count.ToString());
            node.NonPlayerCharacter.Name = "NPC" + count.ToString();
            AddElement(node);
            return node;
        }

        public StartDialogNode StartDialogNodeCreate(string nodename, Dictionary<string, StartDialogNode> nodes)
        {
            window.SaveNodeFile(true);
            var node = new StartDialogNode(this, nodes) { title = nodename };
            node.StartDialog.NodeID = Guid.NewGuid().ToString() + DateTime.Now.Ticks;
            node.SetTitle("Start Node");
            node.StartDialog.Name = "Start Node";
            AddElement(node);
            return node;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // กำหนดเงื่อนไขการเชื่อมต่อระหว่าง ports
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

    }
}
#endif