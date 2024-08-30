using TMPro;
using UnityEngine;

namespace com.playbux.ui
{
    public class DynamicTextContainerSize : MonoBehaviour
    {
        [SerializeField]
        private float maxWidth = 80f; // Maximum width before text wraps

        [SerializeField]
        private float horizontalPadding = 80;

        [SerializeField]
        private float verticalPadding = 20;

        [SerializeField]
        private RectTransform background;

        [SerializeField]
        private TextMeshProUGUI textComponent;

        private void Update()
        {
            if (textComponent != null && background != null)
            {
                if (string.IsNullOrEmpty(textComponent.text))
                    return;

                // Get the rendered size of the text
                // Vector2 renderedSize = textComponent.preferredWidth;

                // Clamp the width to the maximum width
                float backgroundWidth = Mathf.Min(textComponent.preferredWidth + horizontalPadding, maxWidth + horizontalPadding);

                // Adjust the background width
                background.sizeDelta = new Vector2(backgroundWidth, textComponent.preferredHeight + verticalPadding);
            }
        }
    }
}