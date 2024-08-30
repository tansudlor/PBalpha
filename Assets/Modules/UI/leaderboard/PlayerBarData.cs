using com.playbux.networking.mirror.client.building;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui.leaderboard
{
    public class PlayerBarData : MonoBehaviour
    {
        [SerializeField]
        private CTEFlagDatabase c2eFlagDatabase;
        [SerializeField]
        private Image medal;
        [SerializeField]
        private Image emoji;
        [SerializeField]
        private Image coin;
        [SerializeField]
        private Sprite[] medalPics;
        [SerializeField]
        private Color[] colorRank;
        [SerializeField]
        private Image colorBar;
        [SerializeField]
        private TextMeshProUGUI rankNum;
        [SerializeField]
        private TextMeshProUGUI namePlayer;
        [SerializeField]
        private TextMeshProUGUI emailPlayer;
        [SerializeField]
        private TextMeshProUGUI pointPlayer;

#if !SERVER

        public void SetData(JToken jsondata,bool isC2E = false)
        {

            //JObject jObjectData = JsonConvert.DeserializeObject<JObject>(jsondata);  
            //Debug.Log(JsonConvert.SerializeObject(jsondata));
            string name = jsondata["display_name"].ToString();
            string email = jsondata["email"].ToString();
            string score = jsondata["total_score"].ToString();
            try
            {
                email = email[0..2] + "***" + email[email.IndexOf('@')..(email.IndexOf('@') + 1)] + "***." + email.Split('.')[email.Split('.').Length - 1];
            }
            catch 
            {
                
            }
            if (!isC2E)
            {
                try
                {

                    if (name.Length > 10)
                    {
                        name = name[0..9];
                    }

                }
                catch
                {

                }
            }

            namePlayer.text = name;
            emailPlayer.text = email;
            pointPlayer.text = score;

        }

        public void SetPlace(int num,bool isPlayer = false)
        {
            
            medal.gameObject.SetActive(false);
            rankNum.gameObject.SetActive(false);
            if (num <= 2)
            {
                medal.gameObject.SetActive(true);
                medal.sprite = medalPics[num];
                pointPlayer.fontSize = 18f;

                if (isPlayer)
                {
                    return;
                }

                colorBar.color = colorRank[num];
            }
            else
            {
                rankNum.text = (num + 1).ToString();
                pointPlayer.fontSize = 18f;
                rankNum.gameObject.SetActive(true);
            }

        }



        public void CoinSet(Sprite sprite)
        {
            coin.sprite = sprite;
        }

        public void IconSet(string flagName)
        {
            flagName = flagName.ToLower();
            string newFlagName = flagName[0].ToString().ToUpper() + flagName[1..];
            flagName = newFlagName;
            //Debug.Log(flagName);

            Texture2D texture2D = c2eFlagDatabase.Get(flagName);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
            emoji.sprite = sprite;
        }
#endif
    }
}
