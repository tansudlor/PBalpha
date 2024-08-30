
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.kicktowin
{
    public class KickCoinController : MonoBehaviour
    {
        private Vector3 targerPosition;

        private Vector3 startPosition;
        public Vector3 TargerPosition { get => targerPosition; set => targerPosition = value; }
        public Vector3 StartPosition { get => startPosition; set => startPosition = value; }

        public System.Action<int> OnFinishMove { get; set; }
       
        private Vector3 controlPoint1;
        private Vector3 controlPoint2;

        private float currentTime = 0f;
        public int Order { get; set; }
        // Update is called once per frame

        private void Start()
        {
            controlPoint1 = StartPosition /*+ (Vector3.left * Random.Range(500f, 1000f))*/ + (Vector3.up * Random.Range(300f, 500f));
            controlPoint2 = TargerPosition + (Vector3.right * Random.Range(100f, 500f)) + (Vector3.down * Random.Range(100f, 500f));
        }

        void Update()
        {
            if (targerPosition == null)
                 return;

            currentTime += Time.deltaTime;

            if(currentTime > 1f)
            {
                OnFinishMove?.Invoke(Order);
                Destroy(gameObject);
                return;
            }

            this.gameObject.transform.localPosition = GetPointOnCubicBezierCurve(StartPosition, controlPoint1 , controlPoint2, TargerPosition, currentTime);    
         
        }

        public  Vector2 GetPointOnCubicBezierCurve(Vector2 startPoint, Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint, float t)
        {
            float oneMinusT = 1 - t;
            float oneMinusTSquared = oneMinusT * oneMinusT;
            float oneMinusTCubed = oneMinusTSquared * oneMinusT;
            float tSquared = t * t;
            float tCubed = tSquared * t;

            Vector2 point = oneMinusTCubed * startPoint; // (1 - t)^3 * P0
            point += 3 * oneMinusTSquared * t * controlPoint1; // 3 * (1 - t)^2 * t * P1
            point += 3 * oneMinusT * tSquared * controlPoint2; // 3 * (1 - t) * t^2 * P2
            point += tCubed * endPoint; // t^3 * P3

            return point;
        }

    }
}
