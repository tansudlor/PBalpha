using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using com.playbux.api;
using com.playbux.identity;

namespace com.playbux.ui.gamemenu
{
    public class QuizTimeBanner : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textQuiz;
        [SerializeField]
        private TextMeshProUGUI textMessage;
        [SerializeField]
        private Image imageQuiz;

        [SerializeField]
        private string[] header;
        [SerializeField]
        private string[] messgae;
        [SerializeField]
        private Sprite[] imageQuizList;
        [SerializeField]
        private Color[] colors;

        private void Start()
        {
            this.gameObject.SetActive(false);
        }

        public void SetBanner(string messageInput, object data = null)
        {
            if (messageInput == "eventannouncement")
            {
                string startIn = (string)data;
                textQuiz.text = header[0];
                textQuiz.color = colors[0];
                imageQuiz.sprite = imageQuizList[0];

                var format = string.Format("Starts in: {0}s", startIn);
                textMessage.text = format;
            }
            else if (messageInput == "begin")
            {
                SetData(1);
                textQuiz.color = colors[0];
                this.gameObject.SetActive(true);
                StartCoroutine(ShowLast5Sec());
                return;
            }
            else if (messageInput == "youwin")
            {
                SetWinBanner(2, data);
                textQuiz.color = colors[0];
            }
            else if (messageInput == "wrong")
            {
                SetData(3);
                textQuiz.color = colors[1];
            }
            else if (messageInput == "miss")
            {
                SetData(4);
                textQuiz.color = colors[1];
            }
            else if (messageInput == "end")
            {
                SetData(5);
                textQuiz.color = colors[0];
            }
            this.gameObject.SetActive(true);

            if (messageInput == "eventannouncement" || messageInput == "end")
            {
                StartCoroutine(Show());
            }

        }

        IEnumerator Show()
        {

            yield return new WaitForSeconds(3f);
            this.gameObject.SetActive(false);
        }

        public void CloseBanner()
        {
            this.gameObject.SetActive(false);
        }

        private void SetData(int i)
        {
            textQuiz.text = header[i];
            textMessage.text = messgae[i];
            imageQuiz.sprite = imageQuizList[i];

        }

        IEnumerator ShowLast5Sec()
        {

            for (int i = 5; i >= 1; i--)
            {

                var format = string.Format("Starts in: {0}s", i);
                textMessage.text = format;
                yield return new WaitForSecondsRealtime(1);

            }
            CloseBanner();
        }

        void SetWinBanner(int i, object data)
        {
            textQuiz.text = header[i];
            imageQuiz.sprite = imageQuizList[i];
            List<Reward> rewards = (List<Reward>)data;
           
            //You’ve earned 10 Pebble , 5 Brk!
            string getReward = "You’ve earned";
            for (int j = 0; j < rewards.Count; j++)
            {
                string type = IdentityDetail.VariableToLower[rewards[j].type];
                string quantity = rewards[j].quantity + "";
                getReward = getReward + " " + quantity + " " + type;

                if (j < rewards.Count - 1)
                {
                    getReward += " ,";
                }
            }

            getReward = getReward + "!";
            textMessage.text = getReward;

        }
    }

}
