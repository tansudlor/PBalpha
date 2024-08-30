using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InboxUIController : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    public void CloseReward()
    {
        
        animator.CrossFade("Close",1);
    }
}
