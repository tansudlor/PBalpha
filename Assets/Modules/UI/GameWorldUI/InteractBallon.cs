using TMPro;
using UnityEngine;

namespace com.playbux.ui.world
{
    public class InteractBallon : MonoBehaviour
    {
        [SerializeField]
        private Transform bubbleTrasform;
        [SerializeField]
        private SpriteRenderer ring;
        [SerializeField]
        private SpriteRenderer circle;
        [SerializeField]
        private TextMeshPro Text;
       

        public void ChangeIcon(string text, Color ringColor, Color circleColor)
        {
            Text.text = text;
            ring.color = ringColor;
            circle.color = circleColor;
        }

        public void ChangePosition( Vector3 position)
        {
            //bubbleTrasform.localScale = scale;
            bubbleTrasform.position = position;
        }

        public void Show()
        {
            bubbleTrasform.gameObject.SetActive(true);
        }

        public void Hide()
        {
            bubbleTrasform.gameObject.SetActive(false);
        }

    }

}

