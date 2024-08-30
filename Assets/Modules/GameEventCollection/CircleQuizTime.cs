using com.playbux.networking.mirror.message;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.gameeventcollection
{
    public class CircleQuizTime : MonoBehaviour
    {
        // Start is called before the first frame update
#if !SERVER
        void Start()
        {

            /*var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;*/

        }

        /*private void OnTriggerEnter2D(Collider2D collision)
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
