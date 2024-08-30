using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.playbux.ui.gamemenu
{
    public class LinkClick : MonoBehaviour , IPointerClickHandler
    {
        private string forgetPassURL = "https://playbux-account.insitemedia.co.th/forgot-password";
        private string crateAccountURL = "https://playbux-account.insitemedia.co.th/register";
        private string logInURL = "https://playbux-account.insitemedia.co.th/login?serviceUid=0847cbf0-1c39-40b0-8de0-98b2987cba63";

        [SerializeField]
        private TMP_Text text;

        public void OnPointerClick(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            
            if (linkIndex < 0)
            {
                return;
            }


            var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

            
            var url = linkId switch
            {
                "forgetpassword" => forgetPassURL,
                "createaccount" => crateAccountURL,
                "login" => logInURL,
                _ => null
            };

            Debug.Log($"URL clicked: linkInfo[{linkIndex}].id={linkId}   ==>   url={url}");

            // Let's see that web page!
            Application.OpenURL(url);
        }

       
       
    }
}
