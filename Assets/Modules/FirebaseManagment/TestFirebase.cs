using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.firebaseservice
{
    public class TestFirebase : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FirebaseRemoteConfigManager.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
