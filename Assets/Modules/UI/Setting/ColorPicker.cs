using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.playbux.ui.setting
{

    public class ColorPicker : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject colorPicker;
        [SerializeField]
        private RawImage colorImage;

        private bool over = false;
        private bool isOpenColorPicker = true;
        private ColorPickerObserver target;


        public void ToggleColorPicker(ColorPickerObserver target)
        {

            if (target != this.target)
            {
                isOpenColorPicker = true;
            }

            this.target = target;
            colorPicker.SetActive(isOpenColorPicker);
            isOpenColorPicker = !isOpenColorPicker;
            Debug.Log(this.target + " x " + isOpenColorPicker);

        }


        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log(over + " in pointer Down");
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(colorImage.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            float rectWidth = colorImage.rectTransform.rect.width;
            float rectHeight = colorImage.rectTransform.rect.height;

            float normalizedX = (localCursor.x / (rectWidth) * colorImage.uvRect.width);
            float normalizedY = (localCursor.y / (rectHeight) * colorImage.uvRect.height);

            //Debug.Log(normalizedX + " , " + normalizedY);
            //Debug.Log(((Texture2D)colorImage.texture).GetPixel(Mathf.RoundToInt(normalizedX * colorImage.texture.width), Mathf.RoundToInt(normalizedY * colorImage.texture.height)));
            var colorThis = ((Texture2D)colorImage.texture).GetPixel(Mathf.RoundToInt(normalizedX * colorImage.texture.width), Mathf.RoundToInt(normalizedY * colorImage.texture.height));
            target.SetColorFromPicker(colorThis);
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (over == false)
                {
                    colorPicker.SetActive(false);
                    isOpenColorPicker = true;
                    target = null;
                }

            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            over = true;
            Debug.Log("in pointer Enter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            over = false;
            Debug.Log("in pointer Exit");
        }


    }
}