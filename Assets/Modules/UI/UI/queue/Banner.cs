using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using com.playbux.api;

namespace com.playbux.ui
{
    public class Banner : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI bannerText;
        [SerializeField]
        private GameObject dotData;
        [SerializeField]
        private Sprite whiteDot;
        [SerializeField]
        private Sprite blackDot;

        [SerializeField]
        private TextDataForWaitQueue[] textDataForWaitQueues;

        [SerializeField]
        private GameObject choiceDot;


        private GameObject[] dots;


        private string url;


        private void Start()
        {
            dots = new GameObject[textDataForWaitQueues.Length];
           
            for (int j = 0; j < textDataForWaitQueues.Length; j++)
            {
                dots[j] = Instantiate(choiceDot, dotData.transform);
                dots[j].GetComponent<Image>().sprite = blackDot;
            }

            

            StartCoroutine(RunBanner());
        }

        void SetColor(int index)
        {
            for (int j = 0; j < textDataForWaitQueues.Length; j++)
            {
                dots[j].GetComponent<Image>().sprite = blackDot;
            }

            dots[index].GetComponent<Image>().sprite = whiteDot;
        }



        IEnumerator RunBanner()
        {
           
            int i = 0;
            while (true)
            {
                //Debug.Log("Reun");
                string displayText = textDataForWaitQueues[i % textDataForWaitQueues.Length].SponserData;
                url = textDataForWaitQueues[i % textDataForWaitQueues.Length].Url;
                bannerText.text = displayText;
                SetColor(i % textDataForWaitQueues.Length);
                yield return new WaitForSeconds(5f);
                i++;

            }

        }

        public void OnLinkClick()
        {
            Debug.Log(url);
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
           
            Application.OpenURL(url);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            TMP_Text text = bannerText.GetComponent<TMP_Text>();

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

            if (linkIndex < 0)
            {
                return;
            }

            var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

            Debug.Log($"URL clicked: linkInfo[{linkIndex}].id={linkId}   ==>   url={url}");

            Application.OpenURL(url);
        }

       

    }
}
