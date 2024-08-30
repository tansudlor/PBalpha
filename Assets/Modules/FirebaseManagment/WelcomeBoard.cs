using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;


namespace com.playbux.firebaseservice
{
    public class WelcomeBoard : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        private void Start()
        {
#if !UNITY_EDITOR
            StartCoroutine(WaitForThisActive());
#endif
        }

       

        public void ChangeImage(string imgURL)
        {
            if (string.IsNullOrEmpty(imgURL))
            {
                return;
            }
            StartCoroutine(DownloadImage(imgURL));
        }

        IEnumerator WaitForThisActive()
        {
            yield return new WaitUntil(() => gameObject.activeInHierarchy);
            FirebaseAuthenticationService.GetInstance().SubWelcomeBoard(this);
        }

        IEnumerator DownloadImage(string imgURL)
        {

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imgURL))
            {
                Debug.Log("[WelcomeBoard] : " + imgURL);
                yield return request.SendWebRequest();
                Debug.Log("[WelcomeBoard] : " + imgURL);
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + request.error);
                }
                else
                {
                    Debug.Log("[WelcomeBoard] : ");
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    spriteRenderer.sprite = sprite;
                }
            }
        }


    }
}
