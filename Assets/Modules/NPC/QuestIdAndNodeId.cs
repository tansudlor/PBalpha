namespace com.playbux.npc
{
    public struct QuestIdAndNodeId
    {
        private string questId;
        private string nodeId;

        public string QuestId { get => questId; set => questId = value; }
        public string NodeId { get => nodeId; set => nodeId = value; }

        public QuestIdAndNodeId(string questId, string nodeId)
        {
            this.questId = questId;
            this.nodeId = nodeId;
        }
    }
}
