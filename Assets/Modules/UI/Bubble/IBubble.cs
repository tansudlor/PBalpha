using UnityEngine;

namespace com.playbux.ui.bubble
{
    public interface IBubble
    {
        public void Show();
        public void Hide();
        public void UpdateText(string message);
        public void UpdatePosition(Vector3 position);
        public void Initialize(string message, Vector3 position, Transform parent);
        public void Dispose();


    }
}