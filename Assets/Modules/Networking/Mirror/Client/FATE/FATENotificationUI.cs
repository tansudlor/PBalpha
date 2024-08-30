using com.playbux.ui;
using TMPro;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.client.FATE
{
    public class FATENotificationUI
    {
        private readonly UICanvas canvas;
        private readonly Transform transform;
        private readonly GameObject gameObject;
        private readonly TextMeshProUGUI messageText;
        
        public FATENotificationUI(UICanvas canvas, Transform transform, TextMeshProUGUI messageText)
        {
            this.canvas = canvas;
            this.transform = transform;
            this.messageText = messageText;
            gameObject = transform.gameObject;
        }
        
        public void Initialize(string message)
        {
            transform.SetParent(canvas.transform);
            messageText.text = message;
            gameObject.SetActive(true);
        }

        public void Dispose()
        {
            gameObject.SetActive(false);
        }

        public class Factory : PlaceholderFactory<FATENotificationUI>
        {
            
        }
    }
}