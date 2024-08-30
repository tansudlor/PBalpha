using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.ui.gamemenu
{
    public class MiniMapController : MonoBehaviour
    {

        public GameObject BigMap;
        public void OpenBigMap()
        {
            // BigMap.SetActive(true);
        }

        public void CloseBigMap()
        {
            BigMap.SetActive(false);
        }
        public void Update()
        {

            gameObject.SetActive(false);

        }

    }
}
