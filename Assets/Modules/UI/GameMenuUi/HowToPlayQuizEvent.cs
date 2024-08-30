using com.playbux.sfxwrapper;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace com.playbux.ui.gamemenu
{
    public class HowToPlayQuizEvent : MonoBehaviour
    {
        [SerializeField]
        private GameObject howToPlayGO;
        [SerializeField]
        private Sprite[] buttonSprites;
        [SerializeField]
        private Toggle dontShowToggle;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private Image closeButtonImage;
        [SerializeField]
        private VideoPlayer videoPlayer;

        private bool isWait = true;
        private TweenerCore<Vector3, Vector3, VectorOptions> howToPlayAnim = null;
        private void Start()
        {
            howToPlayGO.transform.localScale = Vector3.zero;
            howToPlayGO.SetActive(false);
        }

        public void ShowHowToPlay()
        {
            string value = PlayerPrefs.GetString("HowToPlayQuiz");
            if (value == "Hide")
            {
                return;
            }

            buttonClose.enabled = false;

            if (howToPlayAnim != null)
            {
                howToPlayAnim = null;
            }
            howToPlayGO.gameObject.SetActive(true);
            howToPlayAnim = howToPlayGO.transform.DOScale(Vector3.one * 1.2f, 0.25f)
            .SetEase(Ease.OutCirc)
            .OnComplete(() =>
            {
                howToPlayGO.transform.DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.Linear);
                    
            });



           
            StartCoroutine(buttonCountDown());
        }



        public void CloseHowToPlay()
        {
            if (isWait)
            {
                return;
            }
            SFXWrapper.getInstance().PlaySFX("SFX/Click");

            if (dontShowToggle.isOn == true)
            {
                PlayerPrefs.SetString("HowToPlayQuiz", "Hide");
            }
            else
            {
                PlayerPrefs.SetString("HowToPlayQuiz", "Show");
            }

            if(howToPlayAnim != null)
            {
                howToPlayAnim = null;
            }

            howToPlayAnim = howToPlayGO.transform.DOScale(Vector3.one * 1.2f, 0.25f)
            .SetEase(Ease.OutCirc)
            .OnComplete(() =>
            {
                howToPlayGO.transform.DOScale(Vector3.zero, 0.1f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => 
                    {
                       
                        howToPlayGO.gameObject.SetActive(false); 
                    });
            });
            


        }



        IEnumerator buttonCountDown()
        {
            for (int i = 5; i >= 0; i--)
            {

                closeButtonImage.sprite = buttonSprites[i];
                yield return new WaitForSecondsRealtime(1f);
            }
            yield return null;
            isWait = false;
            buttonClose.enabled = true;
        }



    }
}
