using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace com.playbux.ui.gamemenu
{
    public class InteractButton : MonoBehaviour
    {
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

    }

}
