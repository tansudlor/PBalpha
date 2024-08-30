#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static com.playbux.tool.FlagDatabase;



namespace com.playbux.tool
{
    public partial class QuestGraphViewWindow : EditorWindow
    {
        private QuestGraphView graphView;
        private ListView listView;
        private Label addFlagtext;
        public static string loadPath = "Assets/Modules/Tool/TempSaveFile/Temp";
        public static int count = 0;
        private Dictionary<string, DialogNode> dialogNodeDict;
        private Dictionary<string, NonPlayerCharacterNode> npcNodeDict;
        private Dictionary<string, StartDialogNode> startNodeDict;
        private Dictionary<string, QuestInfoNode> questInfoNodeDict;
        private string[] flagList;
        public static string flagFileName = "";
        public static string flagTemp = "";
        static Button buttonTemp;
        public int npcCount = 0;
        private string flagAPI = "https://script.google.com/macros/s/AKfycbxDQWbtyXHOaCrNtxEtTvcjQ_6Ai58fHSzXDVw5bhbDpfB0OjWrIkXQ5yIqWasaFjMz3w/exec?action=getflag";
        private FlagDatabase flagDatabase;
        private QuestInfoNode questInfoNode;


        [MenuItem("Graph/QuestGraphView")]
        public static void OpenWindow()
        {
            var window = GetWindow<QuestGraphViewWindow>("QuestGraphView");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            npcCount = 0;

            graphView = new QuestGraphView();
            graphView.window = this;
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
            var menuBar = new VisualElement();
            var file = new VisualElement();
            var edit = new VisualElement();
            var addNewFlag = new VisualElement();

            menuBar.style.flexDirection = FlexDirection.Column;
            menuBar.style.alignSelf = Align.FlexEnd;
            menuBar.style.flexShrink = 0;
            menuBar.style.position = Position.Absolute;
            menuBar.style.bottom = 0;
            menuBar.Add(file);
            rootVisualElement.Add(edit);
            rootVisualElement.Add(menuBar);
            rootVisualElement.Add(addNewFlag);

            file.style.flexDirection = FlexDirection.Column;
            edit.style.flexDirection = FlexDirection.Row;
            edit.style.alignSelf = Align.Center;

            addNewFlag.style.flexDirection = FlexDirection.Row;
            addNewFlag.style.alignSelf = Align.Center;

            var save = new Button(() => SaveNodeFile()) { text = "Save..." };
            file.Add(save);
            var load = new Button(() => LoadNodeFile()) { text = "Load..." };
            file.Add(load);
            var run = new Button(() => ExportFile(SaveNodeFile())) { text = "Export..." };
            file.Add(run);

            var add = new Button(() => graphView.CreateNode("New Node", dialogNodeDict)) { text = "Add new QuestNode" };
            edit.Add(add);

            var addNPC = new Button(() => graphView.NonPlayerCharacterNodeCreate("New NPC", npcCount++, npcNodeDict)) { text = "Add new NPC" };
            edit.Add(addNPC);

            var addStartNode = new Button(() => graphView.StartDialogNodeCreate("Start Quest", startNodeDict)) { text = "Add new StartNode" };
            edit.Add(addStartNode);

            var addQuestInfoNode = new Button(() => graphView.CreateQuestInfoNode("Quest Info", questInfoNodeDict)) { text = "Add new QuestInfo" };
            edit.Add(addQuestInfoNode);

            var addFlagFile = new Button(() => LoadText()) { text = "Select Flag File" };
            addNewFlag.Add(addFlagFile);


            buttonTemp = new Button() { text = "" };
            buttonTemp.visible = false;
            rootVisualElement.Add(buttonTemp);
            rootVisualElement.RegisterCallback<MouseMoveEvent>(MouseMove);
            rootVisualElement.RegisterCallback<MouseUpEvent>(e =>
            {
                flagTemp = "";
                buttonTemp.visible = false;
            });


            buttonTemp.style.width = 100;
            buttonTemp.style.height = 50;
            buttonTemp.style.position = Position.Absolute;

            addFlagtext = new Label();
            addFlagtext.text = "Select Flag File";
            addFlagtext.style.alignSelf = Align.Center;
            addFlagFile.style.height = 20;
            addNewFlag.Add(addFlagtext);

            graphView.NodeIndexCreate("Information");


            //graphView = new Dictionary<string, QuestNode>();

        }

        private void MouseMove(MouseMoveEvent evt)
        {
            if (flagTemp != "")
            {
                buttonTemp.visible = true;

                buttonTemp.style.left = Event.current.mousePosition.x;
                buttonTemp.style.top = Event.current.mousePosition.y;

                buttonTemp.text = flagTemp;
            }
        }

        private void LoadText(string path = "")
        {

            EditorApplication.update += GetFlagList;

            /*if (path == "")
            {
                path = EditorUtility.OpenFilePanel("Select a file", "", "asset");
                var fileSplit = path.Split('/');
                var fileName = fileSplit[fileSplit.Length - 1].Split(".");
                ScriptableObject FlagData = Resources.Load<ScriptableObject>(fileName[0]);
                List<string> allFlag = new List<string>();
                for (int i = 0; i < (FlagData as FlagDatabase).Flags.Count; i++)
                {
                    allFlag.Add((FlagData as FlagDatabase).Flags[i].Flag.ToString());
                    Debug.Log(allFlag[i]);
                }
                flagList = allFlag.ToArray();

                FlagDatabase.ScriptToJson((FlagData as FlagDatabase));


            }
            if (path == "")
            {
                return;

            }*/



        }

        void GetFlagList()
        {
            UnityWebRequest www = UnityWebRequest.Get(flagAPI);
            www.SendWebRequest();

            while (!www.isDone)
            {
                // รอการเสร็จสิ้นการดึงข้อมูล
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {

            }
            else
            {

                string jsonString = www.downloadHandler.text;
                Debug.Log(jsonString);
                flagDatabase = JsonConvert.DeserializeObject<FlagDatabase>(jsonString);

                // เข้าถึงข้อมูล Version และ Flags
                int version = flagDatabase.FlagVersion;
                List<AllFlag> flags = flagDatabase.AllFlags;
                List<string> allFlag = new List<string>();

                for (int i = 0; i < flagDatabase.AllFlags.Count; i++)
                {
                    allFlag.Add(flagDatabase.AllFlags[i].Flag);
                    allFlag.Add("*" + flagDatabase.AllFlags[i].Flag);
                }

                flagList = allFlag.ToArray();

                if (listView != null)
                {
                    addFlagtext.Add(listView);
                    addFlagtext.Remove(listView);
                }

                listView = new ListView();
                listView.itemsSource = flagList;
                listView.makeItem = CreateLabel;
                listView.bindItem = (element, i) => (element as Label).text = listView.itemsSource[i].ToString();
                listView.style.height = 100;
                listView.style.alignContent = Align.FlexStart;
                addFlagtext.text = "";
                addFlagtext.Add(listView);

                FlagDatabase.ScriptToJson(flagDatabase);

                EditorApplication.update -= GetFlagList;
            }
        }

        Label CreateLabel()
        {
            var label = new Label();
            label.RegisterCallback<PointerDownEvent>(evt =>
            {
                //Debug.Log(label.text);
                flagTemp = label.text;
            });



            return label;
        }



        private void ExportFile(string path = "")
        {

            List<NonPlayerCharacter> NPCs = new List<NonPlayerCharacter>();
            string json = File.ReadAllText(path);
            SaveFile saveFile = JsonConvert.DeserializeObject<SaveFile>(json);
            QuestData exportJson = ScriptableObject.CreateInstance<QuestData>();
            exportJson.QuestID = saveFile.Information[0].QuestID;
            exportJson.QuestName = saveFile.Information[0].QuestName;
            exportJson.Position = saveFile.Information[0].Position;
            var infoNode = QuestGraphView.informationNode;
            
            for (int i = 0; i < infoNode.outputContainer.childCount; i++)
            {
                if (infoNode.outputContainer[i].GetType() != typeof(Port))
                {
                    continue;
                }

                NPCs.Add(saveFile.NonPlayerCharacters.First(n => n.NodeID == ((Port)infoNode.outputContainer[i]).tooltip));

            }

            exportJson.NonPlayerCharcater = NPCs;
            exportJson.StartDialogs = saveFile.StartDialogs;
            exportJson.Dialogs = saveFile.Dialogs;
            exportJson.QuestInformation = saveFile.QuestInformation;
            exportJson.AllNodeIds = new List<string>();
            exportJson.AllNodeDatas = new List<string>();

            foreach (var node in NPCs)
            {
                exportJson.AllNodeIds.Add(node.NodeID);
                exportJson.AllNodeDatas.Add(JsonConvert.SerializeObject(node));
            }

            foreach (var node in saveFile.StartDialogs)
            {
                exportJson.AllNodeIds.Add(node.NodeID);
                exportJson.AllNodeDatas.Add(JsonConvert.SerializeObject(node));
            }

            foreach (var node in saveFile.Dialogs)
            {
                exportJson.AllNodeIds.Add(node.NodeID);
                exportJson.AllNodeDatas.Add(JsonConvert.SerializeObject(node));
            }

            foreach (var node in saveFile.QuestInformation)
            {
                exportJson.AllNodeIds.Add(node.NodeID);
                exportJson.AllNodeDatas.Add(JsonConvert.SerializeObject(node));
            }


            string exportPath = "Assets/Modules/Tool/QuestExport/Export" + saveFile.Information[0].QuestID + ".asset";
            AssetDatabase.DeleteAsset(exportPath);
            AssetDatabase.CreateAsset(exportJson, exportPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //ทำ Json ด้วยนะ

            var exportToJson = JsonConvert.SerializeObject(exportJson);
            File.WriteAllText("Assets/Modules/Tool/QuestExport/" + saveFile.Information[0].QuestID + ".json", exportToJson);
        }

        public void ClearAllNodes(QuestGraphView graphView)
        {
            // สร้างรายการของโหนดเพื่อหลีกเลี่ยงการแก้ไขคอลเลกชันขณะวนลูป
            var nodes = graphView.nodes.ToList();

            foreach (var node in nodes)
            {
                graphView.RemoveElement(node);
            }

            var edges = graphView.edges.ToList();

            foreach (var edge in edges)
            {
                graphView.RemoveElement(edge);
            }


        }
        public void LoadNodeFile(string path = "")
        {
            if (path == "")
            {
                path = EditorUtility.OpenFilePanel("Select a file", "", "json");
            }
            if (path == "")
            {
                return;
            }
            ClearAllNodes(graphView);

            //var dir = (new FileInfo(path)).Directory;
            var filename = Path.GetFileNameWithoutExtension(path);
            loadPath = loadPath + filename;
            Debug.Log(loadPath);
            string json = File.ReadAllText(path);
            SaveFile saveFile = JsonConvert.DeserializeObject<SaveFile>(json);

            InformationNode infoNode = new InformationNode(graphView, saveFile.Information[0]);
            QuestGraphView.informationNode = infoNode;
            graphView.AddElement(infoNode);
            SetPositionNode(infoNode, saveFile.Information[0].Position);

            Dictionary<string, BaseNode> allNode = new Dictionary<string, BaseNode>();

            for (int i = 0; i < saveFile.QuestInformation.Count; i++)
            {

                var questInfo = saveFile.QuestInformation[i];
                QuestInfoNode questInfoNode = new QuestInfoNode(graphView, questInfoNodeDict);
                graphView.AddElement(questInfoNode);
                SetPositionNode(questInfoNode, questInfo.Position);
                questInfoNode.tooltip = questInfo.NodeID.ToString();
                ((Label)questInfoNode.titleContainer[0]).text = questInfo.Name;

                for (int j = 0; j < questInfo.ProgressFlags.Count; j++)
                {
                    var description = questInfoNode.AddDescription();
                    ((TextField)description[1]).value = questInfo.ProgressFlags[j].QuestDescription;
                    ((ListView)description[3]).itemsSource = AddToList(questInfo.ProgressFlags[j].ActivateFlag);
                    ((ListView)description[5]).itemsSource = AddToList(questInfo.ProgressFlags[j].FinishFlag);
                }

                allNode[questInfo.NodeID] = questInfoNode;


            }


            for (int i = 0; i < saveFile.NonPlayerCharacters.Count; i++)
            {
                var npc = saveFile.NonPlayerCharacters[i];
                NonPlayerCharacterNode npcNode = new NonPlayerCharacterNode(graphView, npcNodeDict, npc);
                graphView.AddElement(npcNode);
                SetPositionNode(npcNode, npc.Position);
                npcNode.tooltip = npc.NodeID.ToString();
                npcNode.title = npc.Name;
                allNode[npc.NodeID] = npcNode;
            }

            for (int i = 0; i < saveFile.StartDialogs.Count; i++)
            {
                var startDialog = saveFile.StartDialogs[i];
                StartDialogNode startDialogNode = new StartDialogNode(graphView, startNodeDict, startDialog);
                graphView.AddElement(startDialogNode);
                SetPositionNode(startDialogNode, startDialog.Position);
                startDialogNode.tooltip = startDialog.NodeID.ToString();
                startDialogNode.title = startDialog.Name;
                allNode[startDialog.NodeID] = startDialogNode;
            }

            for (int i = 0; i < saveFile.Dialogs.Count; i++)
            {
                var dialog = saveFile.Dialogs[i];
                DialogNode dialogNode = new DialogNode(graphView, dialogNodeDict, dialog);
                graphView.AddElement(dialogNode);
                SetPositionNode(dialogNode, dialog.Position);
                dialogNode.tooltip = dialog.NodeID.ToString();
                dialogNode.title = dialog.Name;
                allNode[dialog.NodeID] = dialogNode;
            }

            for (int i = 0; i < saveFile.Edges.Count; i++)
            {
                BaseNode selected;
                Port outputPort = null;
                if (saveFile.Edges[i].StartNode != "")
                {
                    selected = allNode[saveFile.Edges[i].StartNode];
                    if (saveFile.Edges[i].EndNode == "Unlinked")
                    {
                        outputPort = selected.AddOutputPort("??");

                        if (selected.GetType() == typeof(DialogNode))
                        {
                            outputPort.portName = ((DialogNode)selected).Dialog.Choices[saveFile.Edges[i].StartIndex - 1].Message;
                        }

                        continue;
                    }
                    if (selected.GetType() == typeof(StartDialogNode))
                    {
                        outputPort = (Port)selected.outputContainer[0];

                    }
                    else if (selected.GetType() == typeof(NonPlayerCharacterNode))
                    {

                        outputPort = selected.ApplyPort("", saveFile.Edges[i].StartIndex);
                    }
                    else if (selected.GetType() == typeof(DialogNode))
                    {

                        outputPort = selected.ApplyPort(((DialogNode)selected).Dialog.Choices[saveFile.Edges[i].StartIndex - 1].Message, saveFile.Edges[i].StartIndex);
                    }

                    else if (selected.GetType() == typeof(QuestInfoNode))
                    {
                        outputPort = (Port)selected.outputContainer[0];
                    }
                }
                else
                {
                    selected = infoNode;
                    if (saveFile.Edges[i].EndNode == "Unlinked")
                    {
                        outputPort = selected.AddOutputPort();
                        continue;
                    }
                    else
                    {
                        outputPort = selected.ApplyPort("", saveFile.Edges[i].StartIndex);
                    }
                }
                Edge edge = outputPort.ConnectTo((Port)(allNode[saveFile.Edges[i].EndNode].inputContainer[0]));
                // Add edge to the graph
                graphView.AddElement(edge);

                List<Edge> edges = new List<Edge>
                {
                    edge
                };
                GraphViewChange graphViewChange = new GraphViewChange();
                graphViewChange.edgesToCreate = edges;

                graphView.OnGraphViewChanged(graphViewChange);
            }

        }

        public string SaveNodeFile(bool auto = false)
        {

            List<Dialog> dialogs = new List<Dialog>();
            List<NonPlayerCharacter> nonPlayerCharacters = new List<NonPlayerCharacter>();
            List<Information> infomations = new List<Information>();
            List<StartDialog> startDialogs = new List<StartDialog>();
            List<QuestInformation> questInfomation = new List<QuestInformation>();
            List<EdgeData> edgeDatas = new List<EdgeData>();
            SaveFile saveFile = new SaveFile();

            foreach (var node in graphView.nodes)
            {
                if (node.GetType() == typeof(DialogNode))
                {
                    ((DialogNode)node).Dialog.PrepareSave(node);
                    for (int i = 0; i < ((DialogNode)node).Dialog.Choices.Count; i++)
                    {
                        if (((DialogNode)node).Dialog.Choices[i].Message == "[DELETED]")
                        {
                            ((DialogNode)node).Dialog.Choices.RemoveAt(i);
                        }
                    }
                    dialogs.Add(((DialogNode)node).Dialog);

                    for (int i = 0; i < ((DialogNode)node).outputContainer.childCount; i++)
                    {
                        var thisDialogPort = ((DialogNode)node).outputContainer[i];
                        if (thisDialogPort.GetType() == typeof(Port))
                        {
                            if (((Port)thisDialogPort).portName == "[DELETED]")
                            {

                                continue;
                            }
                            EdgeData edgeData = new EdgeData
                            {
                                StartNode = ((DialogNode)node).tooltip,
                                EndNode = "Unlinked",
                                StartIndex = i,
                                EndIndex = -1

                            };
                            edgeDatas.Add(edgeData);
                        }

                    }

                }
                if (node.GetType() == typeof(NonPlayerCharacterNode))
                {
                    ((NonPlayerCharacterNode)node).NonPlayerCharacter.PrepareSave(node);
                    //Debug.Log(JsonConvert.SerializeObject(((NonPlayerCharacterNode)node).NonPlayerCharacter.StartDialogList));
                    for (int i = 0; i < ((NonPlayerCharacterNode)node).NonPlayerCharacter.StartDialogList.Count; i++)
                    {
                        if (String.IsNullOrEmpty(((NonPlayerCharacterNode)node).NonPlayerCharacter.StartDialogList[i]))
                        {
                            ((NonPlayerCharacterNode)node).NonPlayerCharacter.StartDialogList.RemoveAt(i);
                        }
                    }

                    nonPlayerCharacters.Add(((NonPlayerCharacterNode)node).NonPlayerCharacter);

                    for (int i = 0; i < ((NonPlayerCharacterNode)node).outputContainer.childCount; i++)
                    {
                        var thisNonPlayerCharcaterPort = ((NonPlayerCharacterNode)node).outputContainer[i];
                        if (thisNonPlayerCharcaterPort.GetType() == typeof(Port))
                        {
                            if (((Port)thisNonPlayerCharcaterPort).portName == "[DELETED]")
                            {
                                continue;
                            }
                            EdgeData edgeData = new EdgeData
                            {
                                StartNode = ((NonPlayerCharacterNode)node).tooltip,
                                EndNode = "Unlinked",
                                StartIndex = i,
                                EndIndex = -1

                            };
                            edgeDatas.Add(edgeData);
                        }

                    }

                }
                if (node.GetType() == typeof(InformationNode))
                {

                    var info = ((InformationNode)node).Information;
                    info.PrepareSave(node);
                    info.NonPlayerCharacterIDs = new List<string>();
                    for (int i = 0; i < node.outputContainer.childCount; i++)
                    {
                        if ((((InformationNode)node).outputContainer[i]).GetType() == typeof(Port))
                        {
                            info.NonPlayerCharacterIDs.Add(((Port)node.outputContainer[i]).tooltip);
                        }
                    }
                    infomations.Add(info);

                    for (int i = 0; i < ((InformationNode)node).outputContainer.childCount; i++)
                    {
                        var thisInfoPort = ((InformationNode)node).outputContainer[i];
                        if (thisInfoPort.GetType() == typeof(Port))
                        {
                            if (((Port)thisInfoPort).portName == "[DELETED]")
                            {
                                continue;
                            }
                            EdgeData edgeData = new EdgeData
                            {
                                StartNode = ((InformationNode)node).tooltip,
                                EndNode = "Unlinked",
                                StartIndex = i,
                                EndIndex = -1

                            };
                            edgeDatas.Add(edgeData);
                        }

                    }
                }
                if (node.GetType() == typeof(StartDialogNode))
                {
                    ((StartDialogNode)node).StartDialog.PrepareSave(node);
                    startDialogs.Add(((StartDialogNode)node).StartDialog);
                }

                if (node.GetType() == typeof(QuestInfoNode))
                {
                    var infoQuestNode = (QuestInfoNode)node;
                    var questInfo = infoQuestNode.QuestInformation;
                   
                    questInfo.NextInfo = infoQuestNode.outputContainer[0].tooltip;
                    List<ProgressFlag> progressList = new List<ProgressFlag>();
                    questInfo.Name = ((Label)infoQuestNode.titleContainer[0]).text;
                    questInfo.NodeID = infoQuestNode.tooltip;
                    for (int i = 0; i < infoQuestNode.extensionContainer.childCount; i++)
                    {
                        ProgressFlag progressFlag = new ProgressFlag();
                        var questStep = infoQuestNode.extensionContainer[i];
                        string finishFlagToString = AddToString((List<String>)(((ListView)questStep[5]).itemsSource));
                        string activteFlagToString = AddToString((List<String>)(((ListView)questStep[3]).itemsSource));
                        progressFlag.FinishFlag = finishFlagToString;
                        progressFlag.ActivateFlag = activteFlagToString;
                        progressFlag.QuestDescription = ((TextField)questStep[1]).text;
                        progressList.Add(progressFlag);
                    }

                    questInfo.ProgressFlags = progressList;
                    questInfo.PrepareSave(node);
                    questInfomation.Add(questInfo);
                }
            }

            foreach (var edge in graphView.edges)
            {
                var outputIndex = 0;
                var inputIndex = 0;
                for (int i = 0; i < edge.output.node.outputContainer.childCount; i++)
                {
                    if (edge.output.node.outputContainer[i] == edge.output)
                    {
                        outputIndex = i;
                    }
                }
                for (int i = 0; i < edge.input.node.outputContainer.childCount; i++)
                {
                    if (edge.input.node.outputContainer[i] == edge.input)
                    {
                        inputIndex = i;
                    }
                }
                EdgeData edgeData = new EdgeData
                {
                    StartNode = edge.output.node.tooltip,
                    EndNode = edge.input.node.tooltip,
                    StartIndex = outputIndex,
                    EndIndex = inputIndex

                };
                edgeDatas.Add(edgeData);
            }

            saveFile.Information = infomations;
            saveFile.NonPlayerCharacters = nonPlayerCharacters;
            saveFile.StartDialogs = startDialogs;
            saveFile.QuestInformation = questInfomation;
            saveFile.Dialogs = dialogs;
            saveFile.Edges = edgeDatas;

            string json = JsonConvert.SerializeObject(saveFile);

            // Prompt the user to select a file path to save
            string path = loadPath + (count++) % 100 + ".json";

            if (!auto)
            {
                path = EditorUtility.SaveFolderPanel("Selet Output Folder", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Write the JSON data to the file
                    File.WriteAllText(path + "/" + saveFile.Information[0].QuestID + ".json", json);
                    return path + "/" + saveFile.Information[0].QuestID + ".json";
                }
            }
            else
            {
                File.WriteAllText(path, json);
            }
            return path;
        }

        void SetPositionNode(Node node, string pos)
        {
            var spl = pos.Split(',');
            node.SetPosition(new Rect(float.Parse(spl[0]), float.Parse(spl[1]), 500, 500));
        }


        static public Choice GetChoice(UnityEditor.Experimental.GraphView.Port port)
        {
            //Debug.Log(port);
            for (int i = 0; i < port.node.outputContainer.childCount; i++)
            {

                //Debug.Log("-" + port.node.outputContainer[i]);
                if (port.node.outputContainer[i] == port)
                {
                    DialogNode node = (DialogNode)port.node;
                    // Debug.Log(node.title + node.Choices.Count + "," + (i - 1));
                    /*if (node.Choices.Count < (i))
                    {
                        node.Choices.Add(new Choice());
                    }
                    return node.Choices[i - 1];*/
                }
            }

            return null;
        }

        public List<string> AddToList(string allFlag)
        {
            return allFlag.Split(new String[] { "■■" }, StringSplitOptions.None).ToList();
        }

        public string AddToString(List<string> listFlag)
        {
            return string.Join("■■", listFlag);
        }
    }
}
#endif