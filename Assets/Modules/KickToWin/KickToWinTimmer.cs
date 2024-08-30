
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.kicktowin
{
    public class KickToWinTimmer : MonoBehaviour
    {

        [SerializeField]
        private GameObject bar;
        [SerializeField]
        private GameObject fillBar;
        [SerializeField]
        private GameObject clock;
        [SerializeField]
        private Image fillbarImage;

       

        public long StayTicks = 0 ;
        public long ArrivalTicks = 0;
        private long clientStayTicks = 0;
        private long successTicks = 30_000_000;
        private Vector3 clockScale = Vector3.one;
        private Vector3 gaugeScale = Vector3.one;

       
#if !SERVER
        private void Start()
        {
            gameObject.SetActive(true);
            fillbarImage.fillAmount = 0;
        }
        private void Update()
        {
            clock.transform.localScale += (clockScale - clock.transform.localScale) / 8;
            gameObject.transform.localScale += (gaugeScale - gameObject.transform.localScale) / 8;
            if (fillbarImage.fillAmount >= 1f)
            {
                StayTicks = 0;
                clientStayTicks = 0;
                ArrivalTicks = 0;
                return;
            }


            if(ArrivalTicks != 0)
            {
                clientStayTicks = StayTicks + (DateTime.UtcNow.Ticks - ArrivalTicks);
                fillbarImage.fillAmount = (clientStayTicks * 1.0f) / (successTicks * 1.0f);
               
            }

        
        }

        public IEnumerator DestoryTimmer()
        {
            yield return null;
            clockScale = Vector3.one * 1.75f;
            yield return new WaitForSeconds(0.3f);
            clockScale = Vector3.one;
            yield return new WaitForSeconds(0.3f);
            gaugeScale = Vector3.one * 0.75f;
            yield return new WaitForSeconds(0.3f);
            gaugeScale = Vector3.one * 1.75f;
            yield return new WaitForSeconds(0.3f);
            gaugeScale = Vector3.zero;
            yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
        }

        public IEnumerator DestoryTimmerNormal()
        {
            yield return null;
            gaugeScale = Vector3.zero;
            yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
        }
#endif
    }


}
