
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui.gamemenu
{
    public class ChoiceQuizButton : MonoBehaviour
    {
        [SerializeField]
        private Image dot;
        [SerializeField]
        private TextMeshProUGUI choiceText;

       
        public TextMeshProUGUI ChoiceText { get => choiceText; set => choiceText = value; }
        public Image Dot { get => dot; set => dot = value; }
    }
}
