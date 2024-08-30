using com.playbux.avatar;
using com.playbux.events;
using com.playbux.input;
using com.playbux.inventory;
using com.playbux.networking.mirror.message;
using com.playbux.networking.networkavatar;
using com.playbux.ui;
using Cysharp.Threading.Tasks;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using Zenject;
using com.playbux.api;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using com.playbux.sfxwrapper;
using com.playbux.ui.sortable;
using UnityEngine.Serialization;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace com.playbux.networking.networkinventory
{
    public class NftWithFullData
    {
        public InventoryCommunicateData Data;
        public CollectionAndId Index;
        public NftInfo Nft;
        public int RarityIndex;
        public int Count;
    }

    public sealed class InventoryUIController : MonoBehaviour
    {
        public bool Ready { get => ready; set => ready = value; }
        public Transform Content { get => content; }

#if UNITY_EDITOR
        public List<DisplayName> debugAllCollectionMap;
#endif
        public GameObject dialogInfo;

        [SerializeField]
        private GameObject hilight;

        [SerializeField]
        private String tabSelect;

        [SerializeField]
        private Transform content;

        [SerializeField]
        private GameObject itemButton;

        [SerializeField]
        private string baseFolder = "Assets/TemporaryModules/NetworkInventory/UI/Thumbnail/";

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private CloseButton closeButton;

        [SerializeField]
        private GameObject[] body;

        [SerializeField]
        private GameObject[] tab;

        [SerializeField]
        private Sprite[] correctionFrameList;

        [SerializeField]
        private Sprite[] rarityList;

        [SerializeField]
        private ThumbnailCollection[] thumbnailCollections;

        private bool ready;
        private bool active;
        private float lerp = 0;
        private float lastAPITime = 0f;
        private ISortableUI sortableUI;
        private NetworkInventoryModel model;
        private IInputController inputController;
        private List<NftWithFullData> nfts;
        private Dictionary<string, int> rarityMap;
        private Dictionary<string, PartPathMaping> allCollection;
        private Dictionary<string, Dictionary<CollectionAndId, bool>> AllGroup;
        private NetworkAvatarBoard board;
        private Dictionary<string, string> collectionToSuffix;
        private Dictionary<string, string> suffixToCollection;
        private Dictionary<int, string> idToPart = new Dictionary<int, string>
        {
            { 0,"hat"},
            { 1,"head"},
            { 2,"face"},
            { 3,"shirt"},
            { 4,"pants"},
            { 5,"back"},
            { 6,"shoes"}
        };
        private Dictionary<string, int> partToId = new Dictionary<string, int>
        {
            {"hat",0},
            {"head",1},
            {"face",2},
            {"shirt",3},
            {"pants",4},
            {"back",5},
            {"shoes",6}
        };

        private List<InventoryCommunicateData> cacheItemList = new List<InventoryCommunicateData>();

        [Inject]
        private void Construct(
            UICanvas Canvas,
            ISortableUI sortableUI,
            NetworkAvatarBoard board,
            NetworkInventoryModel model,
            IInputController inputController
            )
        {
            this.sortableUI = sortableUI;
            this.inputController = inputController;
            this.inputController.OnReleased += Toggle;

            transform.SetParent(Canvas.transform);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            this.model = model;
            this.board = board;
            collectionToSuffix = new Dictionary<string, string>();
            collectionToSuffix["Early Bird Quest".ToLower()] = "1"; // will be lowercase to 'pbu' by EquipInfo for map with key - dictionary in avatar controller
            collectionToSuffix["Ultra".ToLower()] = "2";// will be lowercase to 'pbu' by EquipInfo for map with key-dictionary in avatar controller

            suffixToCollection = new Dictionary<string, string>();
            suffixToCollection["1".ToLower()] = "Early Bird Quest";
            suffixToCollection["2".ToLower()] = "Ultra";


            rarityMap = new Dictionary<string, int>
            {
                {"normal".ToLower(),0},
                {"rare".ToLower(),1},
                {"super rare".ToLower(),2},
                { "special super rare".ToLower(),3},
            };
        }

        private void OnDestroy()
        {
            inputController.OnReleased -= Toggle;
        }

        private void Start()
        {
            ready = false;
            tabSelect = "hat";
            hilight.transform.position = tab[partToId[tabSelect]].transform.position;
            dialogInfo.SetActive(false);

            allCollection = new Dictionary<string, PartPathMaping>();
            allCollection = thumbnailCollections.SelectMany(tc => tc.Map)
             .ToDictionary(pair => pair.Key, pair => pair.Value);
#if UNITY_EDITOR
            debugAllCollectionMap = new List<DisplayName>();
            foreach (var item in allCollection.Keys)
            {
                debugAllCollectionMap.Add(new DisplayName(allCollection[item].Path));
            }
#endif
            StartCoroutine(SyncInventory());
            StartCoroutine(RefreshEquiped());

        }

        public void Toggle()
        {
            if (lastAPITime + 10 < Time.time)
            {

                cacheItemList = null;
            }

            lastAPITime = Time.time;
            /*var curretParent = transform.parent;
            this.transform.SetParent(Camera.main.transform);
            this.transform.SetParent(curretParent);*/
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            ConnectAPICheck().Forget();

        }

        private async UniTask ConnectAPICheck()
        {

            if (!active)
            {
                if (cacheItemList != null)
                {

                    FinishList(cacheItemList);
                    board.OnUpdateEquiped = () => { StartCoroutine(RefreshEquiped()); };
                    active = !active;
                    return;

                }

                //connect api
                JObject inventoryData = await APIServerConnector.InventoryAPI(PlayerPrefs.GetString(TokenUtility.accessTokenKey));
                List<InventoryCommunicateData> itemList = new List<InventoryCommunicateData>();

                for (int i = 0; i < inventoryData["inventories"].Count(); i++)
                {

                    var item = inventoryData["inventories"][i];
                    string collection = item["item_detail"]["collection_name"].ToString();
                    string id = item["item_detail"]["token_type"].ToString();
                    int count = item["amount"].ToObject<int>();
                    Debug.Log(collection + " : " + id + " : " + count);
                    itemList.Add(new InventoryCommunicateData(collection, id, count));

                }
                cacheItemList = itemList;
                FinishList(itemList);
                //create data table
                board.OnUpdateEquiped = () => { StartCoroutine(RefreshEquiped()); };
            }
            active = !active;
        }


        public void Close()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            active = false;
        }

        public void OnConversationSignalReceived(ConversationDialogSignal signal)
        {
            active = false;
        }

        private void HandleUIAnimation()
        {
            if (active)
            {
                transform.localScale += (Vector3.one - transform.localScale) / 8;
                canvasGroup.alpha += (1 - canvasGroup.alpha) / 8;
                canvasGroup.interactable = true;
            }
            else
            {
                transform.localScale += (Vector3.one * 0.0001f - transform.localScale) / 8;
                canvasGroup.alpha += (0 - canvasGroup.alpha) / 8;
                canvasGroup.interactable = false;
            }
        }

        private IEnumerator SyncInventory()
        {
            model.OnNftListCount = CreateListView;
            model.OnAdd = ActiveListItem;
            yield return new WaitForSecondsRealtime(3);
            board.OnUpdateEquiped = () => { StartCoroutine(RefreshEquiped()); };
        }

        private void DestroyAllActiveButton()
        {
            itemButton.SetActive(false);
            // Loop through all children and destroy them
            for (int i = Content.childCount - 1; i >= 0; i--)
            {
                var child = Content.GetChild(i);

                if (child.gameObject.activeInHierarchy)
                {
                    child.name = "delete";
                    Destroy(child.gameObject);
                }
            }
        }

        private void CreateListView(int count)
        {
            DestroyAllActiveButton();
        }

        private GameObject CreateButton(NftWithFullData nftinfo)
        {
            GameObject activeItem = Instantiate(itemButton, Content);
            var cell = activeItem.GetComponent<CellButton>();
            cell.name = nftinfo.Nft.Name;
            cell.Fulldata = nftinfo;
            cell.Controller = this;
            cell.CollectionToSuffix = collectionToSuffix;
            cell.Board = board;
            cell.InfoDialog = dialogInfo.GetComponent<InfoDialog>();
            cell.OnButtonClicked += sortableUI.BringToTop;
            activeItem.transform.position = itemButton.transform.position;
            activeItem.transform.localScale = itemButton.transform.localScale;
            activeItem.SetActive(true);
            return activeItem;
        }

        private (GameObject, NftWithFullData) CreateEquiped(string id_suffix, Transform parent)
        {
            var goNft = parent.Find("NFT");
            if (goNft != null)
            {
                goNft.name = "delete";
                Destroy(goNft.gameObject);
            }

            if (id_suffix == null)
                return (null, null);

            // make key
#if DEVELOPMENT
            Debug.Log(id_suffix);
#endif


            var key = GetEquipID(id_suffix);

            if (key.Id == "default")
                return (null, null);

#if DEVELOPMENT
            Debug.Log(id_suffix);
            Debug.Log(key.Collection);
            Debug.Log(key.Id);
#endif
            var data = new InventoryCommunicateData(key.Collection, key.Id, 0);
            // make fulldata
            NftWithFullData nft = new NftWithFullData();
            nft.Data = data;
            nft.Index = key;
            nft.Count = data.Count;
            nft.Nft = model.GetInfo()[key];
            nft.RarityIndex = rarityMap[model.GetInfo()[key].Rarity.ToLower()];
            GameObject activeItem = Instantiate(itemButton, parent);
            activeItem.name = "NFT";
            var cell = activeItem.GetComponent<CellButton>();
            cell.equiped = true;
            cell.Fulldata = nft;
            cell.Controller = this;
            cell.CollectionToSuffix = collectionToSuffix;
            cell.Board = board;
            cell.InfoDialog = dialogInfo.GetComponent<InfoDialog>();
            activeItem.transform.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(26.3845f, 44);
            activeItem.transform.localScale = Vector3.one * 0.55f;
            activeItem.SetActive(true);
            return (activeItem, nft);
        }


        private CollectionAndId GetEquipID(string id)
        {
            string[] idAndCollection = id.Split("_");
            var getId = id;
            var getCollection = "Early Bird Quest";
            if (idAndCollection.Length > 1)
            {
                getId = idAndCollection[0];
                getCollection = suffixToCollection[idAndCollection[1].ToLower()];
            }
            //Debug.Log(getId);
            CollectionAndId output = new CollectionAndId(getCollection, getId);
            return output;
        }

        private void ActiveListItem(InventoryCommunicateData data)
        {

        }

        public void Update()
        {
            HandleUIAnimation();

            if (!active)
                return;

            lerp += Time.deltaTime * 2f;
            var rect = hilight.GetComponent<RectTransform>();
            rect.position = Vector3.Lerp(rect.position, tab[partToId[tabSelect]].transform.Find("Button").GetComponent<RectTransform>().position, lerp);
            var image = hilight.GetComponent<Image>();
            if (lerp > 0.7f)
                lerp = 0.7f;
            image.color = new Color(image.color.r, image.color.g, image.color.b, lerp + 0.3f);
        }

        public void TabClicked(string type)
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            lerp = 0;
            tabSelect = type;
            DisplayItem(nfts, type.ToLower());
        }

        private void FinishList(List<InventoryCommunicateData> obj)
        {
            nfts = new List<NftWithFullData>();
            foreach (var data in obj)
            {
                try
                {
                    var key = new CollectionAndId(data.Collection, data.Id);
                    NftWithFullData nft = new NftWithFullData();
                    nft.Data = data;
                    nft.Index = key;
                    nft.Count = data.Count;
                    nft.Nft = model.GetInfo()[key];
                    nft.RarityIndex = rarityMap[model.GetInfo()[key].Rarity.ToLower()];
                    nfts.Add(nft);
                    Debug.LogWarning(JsonConvert.SerializeObject(data));
                }
                catch (Exception e)
                {
#if DEVELOPMENT
                    Debug.LogError(e.Message);
                    Debug.LogError(JsonConvert.SerializeObject(data));
#endif
                    continue;
                }
            }

            DisplayItem(nfts, "hat");
        }

        private void DisplayItem(List<NftWithFullData> nfts, string type)
        {
            var viewData = nfts.Where(info => info.Nft.Type.ToLower() == type.ToLower()).OrderByDescending(info => info.RarityIndex)
                .ThenByDescending(info => info.Nft.Collection)
                .ToList();
            ready = true;
            CreateListView(0);
            foreach (var nftinfo in viewData)
            {

                var data = nftinfo.Data;
                if ((data.Id == "80" || data.Id == "119") && data.Collection == "Early Bird Quest")
                    continue;
                var button = CreateButton(nftinfo);
                string idToPath = collectionToSuffix[data.Collection.ToLower()] + "/" + data.Id;
                Texture2D t2d = (Texture2D)allCollection[(baseFolder + idToPath).ToLower()].Data;
                Sprite loadedSprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                Image newitem = button.transform.Find("Thumbnail").Find("New").GetComponent<Image>();
                Destroy(newitem.gameObject);

                Image nft = button.transform.Find("Thumbnail").Find("NFT").GetComponent<Image>();
                nft.sprite = loadedSprite;


                Image rarity = button.transform.Find("Thumbnail").Find("Rarity").GetComponent<Image>();
                var key = new CollectionAndId(data.Collection, data.Id);
                rarity.sprite = rarityList[nftinfo.RarityIndex];
                Image frame = button.transform.Find("Thumbnail").Find("Frame").GetComponent<Image>();

                if (collectionToSuffix[data.Collection.ToLower()] == "2")
                {
                    frame.sprite = correctionFrameList[0];
                    continue;
                }
                Destroy(frame.gameObject);

            }

        }

        public IEnumerator RefreshEquiped()
        {
            yield return new WaitUntil(() => NetworkClient.localPlayer != null);
            IAvatarSet set = board.GetAvatarSet(NetworkClient.localPlayer.netId, null, false);
            foreach (var wareAt in body)
            {
                var wareAtText = wareAt.transform.Find("WareAt").GetComponent<TextMeshProUGUI>();
                wareAtText.text = wareAt.transform.name;
                var (button, fulldata) = CreateEquiped(set[wareAtText.text.ToLower()], wareAt.transform);
                if (button == null)
                    continue;

                var data = fulldata.Data;
                string idToPath = collectionToSuffix[data.Collection.ToLower()] + "/" + data.Id;
                Texture2D t2d = (Texture2D)allCollection[(baseFolder + idToPath).ToLower()].Data;
                Sprite loadedSprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                Image newitem = button.transform.Find("Thumbnail").Find("New").GetComponent<Image>();
                Destroy(newitem.gameObject);

                Image nft = button.transform.Find("Thumbnail").Find("NFT").GetComponent<Image>();
                nft.sprite = loadedSprite;

                Image rarity = button.transform.Find("Thumbnail").Find("Rarity").GetComponent<Image>();
                rarity.sprite = rarityList[fulldata.RarityIndex];
                Image frame = button.transform.Find("Thumbnail").Find("Frame").GetComponent<Image>();
                if (collectionToSuffix[data.Collection.ToLower()] == "2")
                    frame.sprite = correctionFrameList[0];
                else
                    Destroy(frame.gameObject);
            }
        }
    }
}