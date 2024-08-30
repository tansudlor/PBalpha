#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
namespace com.playbux.tool
{
    public abstract class BaseNode : Node
    {
        public abstract Port AddOutputPort(string inputtext = "");
        public virtual Port ApplyPort( string inputText,int portIndex)
        {
            ((Port)this.outputContainer[portIndex]).portName = inputText;
            return ((Port)this.outputContainer[portIndex]);
        }
        
    }
}
#endif