using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace com.playbux.firebaseservice
{
    public class PlaybuxNews : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI description;
        [SerializeField]
        private Image imageDes;

        private FirebaseAuthenticationService firebase;
        public static PlaybuxNews instance;
        private string linkURL;
        // Start is called before the first frame update
        void Start()
        {
            
            gameObject.transform.localScale = Vector3.zero;
            StartCoroutine(WaitForThisActive());

        }

        IEnumerator WaitForThisActive()
        {
            yield return new WaitUntil(() => gameObject.activeInHierarchy);

            firebase = FirebaseAuthenticationService.GetInstance();
            firebase.SubPlaybuxNews(this);
        }


        // Update is called once per frame
        public void SetNewsData(Dictionary<string, string> newsData)
        {
            description.text = newsData["text"];
            linkURL = newsData["link"];
            StartCoroutine(DownloadImage(newsData["image"]));
        }

        public void Open()
        {
            gameObject.transform.localScale = Vector3.one;
        }

        public void ClickLinkButton()
        {
            Application.OpenURL(linkURL);
            Close();
        }

        public void Close()
        {
            Destroy(gameObject);
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

                    imageDes.sprite = sprite;
                }
            }
        }

    }

}
