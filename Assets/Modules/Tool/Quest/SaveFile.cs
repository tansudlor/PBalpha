using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{

    [System.Serializable]
    public class SaveFile
    {
        [SerializeField]
        private List<Information> information;
        [SerializeField]
        private List<NonPlayerCharacter> nonPlayerCharacters;
        [SerializeField]
        private List<StartDialog> startDialogs;
        [SerializeField]
        private List<QuestInformation> questInformation;
        [SerializeField]
        private List<Dialog> dialogs;
        [SerializeField]
        private List<EdgeData> edges ;
        
        
        public List<Information> Information { get => information; set => information = value; }
        public List<NonPlayerCharacter> NonPlayerCharacters { get => nonPlayerCharacters; set => nonPlayerCharacters = value; }
        public List<StartDialog> StartDialogs { get => startDialogs; set => startDialogs = value; }
        public List<Dialog> Dialogs { get => dialogs; set => dialogs = value; }
        public List<EdgeData> Edges { get => edges; set => edges = value; }
        public List<QuestInformation> QuestInformation { get => questInformation; set => questInformation = value; }
    }

    [System.Serializable]
    public class EdgeData
    {
        [SerializeField]
        private string startNode;
        [SerializeField]
        private string endNode;
        [SerializeField]
        private int startIndex;
        [SerializeField]
        private int endIndex;

        public string StartNode { get => startNode; set => startNode = value; }
        public string EndNode { get => endNode; set => endNode = value; }
        public int StartIndex { get => startIndex; set => startIndex = value; }
        public int EndIndex { get => endIndex; set => endIndex = value; }
    }
}
