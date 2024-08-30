using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.networkquest
{
    public class QuestStepController : MonoBehaviour
    {
        public GameObject DescriptionContent { get => descriptionContent; set => descriptionContent = value; }

        [SerializeField]
        private Image closeImage;

        [SerializeField]
        private GameObject descriptionContent;

        [SerializeField]
        private TextMeshProUGUI questName;

        private string descriptionName;

        public void ExpandQuestInfo()
        {
            var status = PlayerPrefs.GetString(descriptionName);
            if (status == "expanded")
            {
                PlayerPrefs.SetString(descriptionName, "!expanded");
                status = "!expanded";
            }
            else
            {
                PlayerPrefs.SetString(descriptionName, "expanded");
                status = "expanded";
            }

            DescriptionContent.SetActive(status == "expanded");

        }
        public void ChangeQuestName(string name)
        {
            descriptionName = name;
            if (!PlayerPrefs.HasKey(descriptionName))
            {
                PlayerPrefs.SetString(descriptionName, "expanded");
            }

            DescriptionContent.SetActive(PlayerPrefs.GetString(descriptionName) == "expanded");
            questName.text = name;
        }
    }
}