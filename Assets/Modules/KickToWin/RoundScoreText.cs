using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.playbux.kicktowin
{
    public class RoundScoreText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreText;

        private int score = 0;
        private Vector3 finalPos = Vector3.zero;
        private Vector3 finalScale = Vector3.zero;
        private float finalAlpha = 0;
        public int Score { get => score; set => score = value; }
        public static bool ClearOld;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Start RoundScoire");
            scoreText.text = "+" + Score.ToString("N0");
            //StartCoroutine(ScoreFloat());
            StartCoroutine(ScoreFade());
            gameObject.AddComponent<Rigidbody2D>().gravityScale = 0;

        }


        IEnumerator ScoreFloat()
        {
            /*finalPos = new Vector3(0f, 40.9f, 0f);
            yield return new WaitForSeconds(0.1f);
            finalScale = Vector3.one * 2f;
            yield return new WaitForSeconds(0.1f);
            finalPos = new Vector3(0f, 75f, 0f);
            finalAlpha = 0.1f;*/
            yield return new WaitForSeconds(1);
            for (int i = 0; i < 50; i++)
            {
                var pos = gameObject.transform.localPosition;
                pos.y += 45f / 50f;
                gameObject.transform.localPosition = pos;
                yield return new WaitForSeconds(0.01f);
            }

        }

        IEnumerator ScoreFade()
        {
            gameObject.transform.localScale = Vector3.one * 2f;
            if(score%5==0)
            {
                ClearOld = true;
            }else
            {
                ClearOld = false;
            }
            gameObject.transform.localPosition += Vector3.up * (Score%5)*10f;
            yield return new WaitForSeconds(0.1f);

            //gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(500, 1000), 800);
            //gameObject.GetComponent<Rigidbody2D>().gravityScale = 200;
            for (int i = 0; i < 10; i++)
            {
                gameObject.transform.localScale *= 0.90f;
                yield return new WaitForSeconds(0.1f);
                if(ClearOld && score % 5 != 0)
                {
                    break;
                }
            }

            Destroy(gameObject);


        }
    }
}
