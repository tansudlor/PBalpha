using com.playbux.avatar;
using com.playbux.networking.networkavatar;
using com.playbux.sfxwrapper;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.networking.networkinventory
{
    public sealed class InfoDialog : MonoBehaviour
    {
        //ref
        [SerializeField] private TextMeshProUGUI[] buttonText;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI chainName;
        [SerializeField] private TextMeshProUGUI rarity;
        [SerializeField] private Image chainIcon;
        [SerializeField] private Image rarityIcon;
        [SerializeField] private Image collection;
        [SerializeField] private GameObject groupChain;
        [SerializeField] private GameObject item;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Sprite[] raritySprites;
        [SerializeField] private Sprite[] chainSprites;
        [SerializeField] private Sprite[] collectionSprites;
        [SerializeField] private NetworkAvatarBoard board;
        [SerializeField] private CellButton cell;
        [SerializeField] private Dictionary<string, string> collectionToSuffix;
        [Inject]
        public void Setup(NetworkAvatarBoard board)
        {
            this.board = board;
        }

        private readonly Dictionary<string, int> collectionIndex = new Dictionary<string, int>
        {
                {"Early Bird Quest".ToLower(),0},
                {"Ultra".ToLower(),1}
        };


        public void EquipItem()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click/Item");
            if (!cell.equiped)
            {
                var itemid = cell.Fulldata.Nft.Id + "_" + collectionToSuffix[cell.Fulldata.Nft.Collection.ToLower()];
                board.ChangePart(NetworkClient.localPlayer.netId, new EquipInfo(cell.Fulldata.Nft.Type, itemid));
                this.gameObject.SetActive(false);
            }
            else
            {
                board.ChangePart(NetworkClient.localPlayer.netId, new EquipInfo(cell.Fulldata.Nft.Type, null));
                this.gameObject.SetActive(false);
            }
        }


        public void SetInfo(string mode, CellButton cell, Dictionary<string, string> collectionToSuffix, string itemName, string rarity, string description, GameObject item, string collection, int rarityIndex, string chainName = "", int chainIndex = 0)
        {
            chainIndex = 1;
            chainName = "BNB Chain";
            this.cell = cell;
            this.collectionToSuffix = collectionToSuffix;
            this.itemName.text = itemName;
            this.chainName.text = chainName;
            this.rarity.text = rarity;
            this.description.text = description;
            this.rarityIcon.sprite = raritySprites[rarityIndex];
            this.rarityIcon.sprite = raritySprites[rarityIndex];
            this.collection.sprite = collectionSprites[collectionIndex[collection.ToLower()]];

            foreach (TextMeshProUGUI text in this.buttonText)
            {
                text.text = mode;
            }
            

            if (chainIndex == 0)
            {
                groupChain.SetActive(false);
                this.chainIcon.enabled = false;
            }
            else
            {
                groupChain.SetActive(true);
                this.chainIcon.enabled = true;
                this.chainIcon.sprite = chainSprites[chainIndex];
            }

            GameObject newitem = Instantiate(item, this.transform);
            Destroy(newitem.GetComponent<CellButton>());
            var rect = newitem.GetComponent<RectTransform>();
            var rectOrigin = this.item.GetComponent<RectTransform>();
            rect.sizeDelta = rectOrigin.sizeDelta;
            rect.pivot = rectOrigin.pivot;
            rect.localScale = rectOrigin.localScale;
            rect.localPosition = rectOrigin.localPosition;
            Destroy(this.item);
            this.item = newitem;
        }
    }
}
