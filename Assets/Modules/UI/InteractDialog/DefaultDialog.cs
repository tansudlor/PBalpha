namespace com.playbux.ui.interactdialog
{
    public class DefaultDialog : IDialog
    {
        private string text;

        public string Text { get => text; set => text = value; }

        public DefaultDialog(string text)
        {
            this.text = text;
        }

        public virtual void Process()
        {

        }
    }

    public class QuestDialog : IDialog
    {
        public string Text { get => text; set => text = value; }
        public string Message { get => message; set => message = value; }

        private string text;
        private string message;
        
        
        public void Process()
        {
            throw new System.NotImplementedException();
        }
    }
}
