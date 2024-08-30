using com.playbux.api;
using ImaginationOverflow.UniversalDeepLinking;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace com.playbux.ui.gamemenu
{
    public class ClientPageStateController : MonoBehaviour
    {
        private OpenAnimation openAnimation;
        // Start is called before the first frame update

        void SetUp(OpenAnimation openAnimation)
        {
            this.openAnimation = openAnimation;
        }

        /*IEnumerator Start()
        {
            
            DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
           
            openAnimation.EnableLogoPageAndVideo();
            var token = TokenUtility.GetToken();
            if (string.IsNullOrEmpty(token.access) && string.IsNullOrEmpty(token.refresh))
            {

            }
            yield return new Wa
        }*/

        // Update is called once per frame
       

        private void Instance_LinkActivated(LinkActivation linkActivation)
        {

            var deepLink = "://";
            var linkSplit = linkActivation.Uri.Split(deepLink);

            var rawQuery = linkActivation.RawQueryString;
          
            var rawQuerySplit = rawQuery.Split("&");
            var token = rawQuerySplit[0].Split("=")[1];
            var refresh = rawQuerySplit[1].Split("=")[1];

            //save accesstoken and refreshtoken to playerpef 
            TokenUtility.SetToken(token, refresh);
            Debug.Log("Instance_LinkActivated");
           

        }



    }
}
