using com.playbux.npc;
using com.playbux.tool;

namespace com.playbux.networkquest
{
    public class DialogData
    {
        private QuestIdAndNodeId key;
        private Dialog dialog;

        public QuestIdAndNodeId Key { get => key; set => key = value; }
        public Dialog Dialog { get => dialog; set => dialog = value; }
    }
}
