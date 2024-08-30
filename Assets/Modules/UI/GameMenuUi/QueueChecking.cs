using com.playbux.api;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace com.playbux.ui.gamemenu
{
    public class QueueChecking : MonoBehaviour
    {

        [SerializeField]
        private TextMeshProUGUI waitNumber;

        public TextMeshProUGUI WaitNumber { get => waitNumber; set => waitNumber = value; }
#if !SERVER
        void Start()
        {
            //Hide();
        }

        // Update is called once per frame
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public async void LeaveQueue()
        {
            await APIServerConnector.LeaveWaitingQueue(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
            GameMenuUiController.ApplicationClose();
            Application.Quit();
        }
#endif

    }

}
