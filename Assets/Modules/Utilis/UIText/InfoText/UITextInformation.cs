using TMPro;
using Zenject;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.utilis.uitext
{
    public class UITextInformation
    {
        private readonly TextMeshProUGUI tmp;
        public UITextInformation(TextMeshProUGUI tmp, UITextInformationSettings settings)
        {
            this.tmp = tmp;
            this.tmp.font = settings.font;
            this.tmp.color = settings.color;
            this.tmp.fontSize = settings.size;

            var styles = new HashSet<int>();

            if (settings.isBold)
                styles.Add((int)FontStyles.Bold);

            if (settings.isItalic)
                styles.Add((int)FontStyles.Italic);

            this.tmp.fontStyle = styles.ToArray().Aggregate(FontStyles.Normal, (current, value) => current | (FontStyles)(value));
        }

        public void SetText(string message) => tmp.text = message;
        public void SetActive(bool enabled) => tmp.gameObject.SetActive(enabled);

        public void SetParent(Transform transform) => tmp.transform.SetParent(transform);

        public class Factory : PlaceholderFactory<UITextInformation>
        {

        }
    }
}