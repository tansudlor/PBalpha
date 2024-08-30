using com.playbux.api;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui.gamemenu
{
    public class TimerCounterController : MonoBehaviour
    {


        [SerializeField]
        private TextMeshProUGUI timerCount;
        [SerializeField]
        private TextMeshProUGUI miliCount;
        private QuizTimeEventUI quizTimeEventUI;
     


        public TextMeshProUGUI TimerCount { get => timerCount; set => timerCount = value; }
        public QuizTimeEventUI QuizTimeEventUI { get => quizTimeEventUI; set => quizTimeEventUI = value; }

        private long prepareCapture = -1;
        // Start is called before the first frame update

        private void Start()
        {
            //this.gameObject.SetActive(true);
            
        }

        private void Update()
        {


            if (prepareCapture < 0)
            {
                return;
            }

            prepareCapture -= (long)(Time.deltaTime * 10_000_000);

            if (prepareCapture > 5*10_000_000)
            {

                prepareCapture = 5 * 10_000_000;
            }


            if (prepareCapture < 0)
            {
                prepareCapture = 0;
                prepareCapture = -1;
                this.gameObject.SetActive(false);
                
            }

            TimerCount.text = String.Format("{0}", (prepareCapture / 10_000_000).ToString("00"));

            miliCount.text = String.Format("{0}",  ((prepareCapture % 10_000_000) / 100_000).ToString("00"));


        }

       


        public void SetCapture(long timer)
        {

            prepareCapture = timer;

        }
    }
}
