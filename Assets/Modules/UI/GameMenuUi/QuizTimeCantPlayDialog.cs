using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.ui.gamemenu
{
    public class QuizTimeCantPlayDialog : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CloseThisDialog();
        }

        // Update is called once per frame
        public void CloseThisDialog()
        {
            gameObject.SetActive(false);
        }

        public void ShowThisDialog()
        {
            gameObject.SetActive(true);
        }
    }
}
