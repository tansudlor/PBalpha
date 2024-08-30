using com.playbux.kicktowin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KICKTOTEST : MonoBehaviour
{
    public GameObject kickToWinUI;
    public GameObject kickCoin;
    public Canvas kickCanvas;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            var kiki = Instantiate(kickCoin, kickCanvas.transform);
            kiki.GetComponent<KickCoinController>().StartPosition = Vector3.zero;
            kiki.GetComponent<KickCoinController>().TargerPosition = kickToWinUI.transform.localPosition;
        }
    }
}
