using UnityEngine;
using Zenject;
using Mirror;

namespace com.playbux.npc
{
    public class ArrowAngleController : MonoBehaviour
    {
        private NPCData npc;

        public NPCData Npc { get => npc; set => npc = value; }

       
        void Start()
        {   
            transform.SetParent(NetworkClient.localPlayer.transform);
            transform.localPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            
            Vector2 targetDirection = this.gameObject.transform.position  -  (npc.NpcPosition + NPCDataBase.Offset);
            ;
            if (targetDirection.magnitude <= 5)
            {
                transform.localScale = Vector3.zero;
                return;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }


    }
}
