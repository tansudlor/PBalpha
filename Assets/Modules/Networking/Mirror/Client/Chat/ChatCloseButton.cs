using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatCloseButton : MonoBehaviour
    {
        private ChatUIController chatUIController;
        
        [Inject]
        public void Construct(ChatUIController chatUIController)
        {
            this.chatUIController = chatUIController;
        }

        public void Close() => chatUIController.Close();
    }
}