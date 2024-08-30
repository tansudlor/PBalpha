using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui.gamemenu
{
    // [ExecuteInEditMode]
    public class DialogTempleteController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private DialogLinkOutData dialogData;

        [SerializeField]
        private GameObject backGround;
        [SerializeField]
        private LayoutElement Layout;

        [SerializeField]
        private Image headerWithText;
        [SerializeField]
        private Image imageContent;
        [SerializeField]
        private TextMeshProUGUI textContent;
        [SerializeField]
        private GameObject confirm;
        [SerializeField]
        private Image imageConfirm;
        [SerializeField]
        private GameObject viewBar;
        [SerializeField]
        private GameObject E;

        [SerializeField]
        private Image imageContentForBorder;
        [SerializeField]
        private TextMeshProUGUI textContentForBorder;
        [SerializeField]
        private GameObject confirmForBorder;
        [SerializeField]
        private GameObject viewBarForBorder;

        [SerializeField]
        private TextMeshProUGUI textCount;

        private IDialogData data;

        public event Action OnAccept;
        public event Action OnClose;

        private int count = 0;
        private void Start()
        {
            SetData(dialogData);
            gameObject.SetActive(false);
            gameObject.transform.localScale = Vector3.zero;
        }
#if UNITY_EDITOR
        /* private void OnValidate()
         {
             SetData(dialogData);
         }*/
#endif
        public void OpenDialogTemplate()
        {
            
#if UNITY_EDITOR
            SetData(dialogData);
#endif  
            gameObject.SetActive(true);
            gameObject.transform.localScale = Vector3.one;
            
            
           
        }

        public void CloseDialogTemplate()
        {
            OnClose?.Invoke();
            count = 0;
            gameObject.SetActive(false);
            gameObject.transform.localScale = Vector3.zero;
        }

        public void SetData(IDialogData dialogData)
        {
            data = dialogData;
            var height = backGround.transform.GetComponent<RectTransform>().rect.height;
            Layout.preferredHeight = height;

            if (dialogData == null)
            {
                headerWithText.sprite = null;
                imageContent.sprite = null;
                imageContentForBorder.sprite = null;
                textContent.text = "";
                textContentForBorder.text = "";
                imageConfirm.sprite = null;
                return;
            }


            headerWithText.sprite = data.Header;
            E.SetActive(false);
            confirm.gameObject.SetActive(false);
            confirmForBorder.SetActive(false);
            viewBar.SetActive(false);
            viewBarForBorder.SetActive(false);

            if (data.ImageContents.Length > 1)
            {
                viewBar.SetActive(true);
                viewBarForBorder.SetActive(true);
                CallImage(0, data.ImageContents[0].texture.width / 2);
                
            }
            else
            {

                CallImage(0, data.ImageContents[0].texture.width / 2);
                confirm.gameObject.SetActive(true);
                confirmForBorder.SetActive(true);
                imageContent.sprite = data.ImageContents[0];
                imageContentForBorder.sprite = data.ImageContents[0];
                textContent.text = data.StringContents[0];
                textContentForBorder.text = data.StringContents[0];

                var newWidth = data.ImageConfirm.textureRect.width / 3;
                imageConfirm.rectTransform.sizeDelta = new Vector2(newWidth, data.ImageConfirm.textureRect.height / 3);
                imageConfirm.sprite = data.ImageConfirm;
            }
        }

        private void Update()
        {
            
            if (data.ImageContents.Length == 1)
            {
                return;

            }
            var height = backGround.transform.GetComponent<RectTransform>().rect.height;
            Layout.preferredHeight = height;
            if (count == 0)
            {
                return;
            }

            if (count < data.ImageContents.Length - 1)
            {
                E.SetActive(false);
                return;
            }

            if (count >= data.ImageContents.Length - 1)
            {

                E.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {

                    CloseDialogTemplate();

                }
            }

        }

        public void PreviousImage()
        {
            count--;
            if (count <= 0)
            {
                count = 0;
            }
            CallImage(count, data.ImageContents[count].texture.width / 2);
        }

        public void NextImage()
        {
            count++;
            if (count > data.ImageContents.Length - 1)
            {
                count = data.ImageContents.Length - 1;
            }

            CallImage(count, data.ImageContents[count].texture.width / 2);
        }

        void CallImage(int i, float contentWidth)
        {
            textCount.text = (i + 1).ToString() + "/" + data.ImageContents.Length;

           
            try
            {

                textContent.text = data.StringContents[i];
                textContentForBorder.text = data.StringContents[i];
                textContent.gameObject.SetActive(true);
                textContentForBorder.gameObject.SetActive(true);

                if (string.IsNullOrEmpty(data.StringContents[i]))
                {
                    textContent.gameObject.SetActive(false);
                    textContentForBorder.gameObject.SetActive(false);
                }

            }
            catch
            {
                textContent.gameObject.SetActive(false);
                textContentForBorder.gameObject.SetActive(false);
            }
            RectTransform rectTransform = backGround.GetComponent<RectTransform>();

            var newWidth = (contentWidth / 864f) * rectTransform.sizeDelta.x;
            var newHeight = ((float)data.ImageContents[i].texture.height / (float)data.ImageContents[i].texture.width) * newWidth;
            imageContent.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            imageContentForBorder.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            imageContent.sprite = data.ImageContents[i];
            imageContentForBorder.sprite = data.ImageContents[i];
        }


        public void OnButtonClick()
        {
            OnAccept?.Invoke();
            Application.OpenURL(data.ConfirmUrl);
            CloseDialogTemplate();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TMP_Text text = textContent.GetComponent<TMP_Text>();

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

            if (linkIndex < 0)
            {
                return;
            }

            var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

            //Debug.Log($"URL clicked: linkInfo[{linkIndex}].id={linkId}   ==>   url={data.ReadMoreUrl}");

            Application.OpenURL(data.ReadMoreUrl);
        }



    }
}
