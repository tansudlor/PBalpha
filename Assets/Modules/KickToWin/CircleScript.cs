using com.playbux.networking.mirror.message;
using Mirror;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.kicktowin
{
    public class CircleScript : MonoBehaviour
    {

        Action<Collider2D> onPlayerEnter;
        Action<Collider2D> onPlayerExit;

        public Action<Collider2D> OnPlayerEnter { get => onPlayerEnter; set => onPlayerEnter = value; }
        public Action<Collider2D> OnPlayerExit { get => onPlayerExit; set => onPlayerExit = value; }
        public SkeletonAnimation BallSportlight { get => ballSportlight; set => ballSportlight = value; }
        public SkeletonAnimation SportLight { get => sportLight; set => sportLight = value; }

        [SerializeField]
        private SkeletonAnimation ballSportlight;

        [SerializeField]
        private SkeletonAnimation sportLight;

        public bool Appear = true;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Appear == false)
            {
                transform.localScale = Vector3.zero;
            }

            /* transform.localScale *= 0.98f;
             if (transform.localScale.x < 0.001f)
             {
                 Destroy(gameObject);
             }*/
        }

#if SERVER
        private void OnTriggerEnter2D(Collider2D collision)
        {
            onPlayerEnter?.Invoke(collision);

        }
        private void OnTriggerExit2D(Collider2D collision)
        {

            onPlayerExit?.Invoke(collision);    

        }
#endif
#if !SERVER
        /*private void OnTriggerStay2D(Collider2D collision)
        {

            if (!NetworkClient.localPlayer.isOwned)
            {
                return;
            }
            Debug.Log("test14123");


            uint netId = collision.gameObject.transform.parent.GetComponent<NetworkIdentity>().netId;
            NetworkClient.Send(new ResyncMessage(netId, "pos", NetworkClient.localPlayer.gameObject.transform.position));


        }*/

#endif
    }

}
