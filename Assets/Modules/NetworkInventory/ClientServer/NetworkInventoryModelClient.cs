#if !SERVER
using com.playbux.inventory;
using com.playbux.networking.mirror.message;
using Cysharp.Threading.Tasks;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.networking.networkinventory
{

    public partial class NetworkInventoryModel
    {
        private List<InventoryCommunicateData> nftList;
        private int nftMaxCount;
        public NetworkInventoryModel(INetworkMessageReceiver<InventoryUpdateMessage> messageReceiver, TextAsset NftDatabase) : base(NftDatabase)
        {
            messageReceiver.OnEventCalled += OnInventoryResponse;
            nftList = new List<InventoryCommunicateData>();
            Log("NIM sub");
            //    test().Forget();

        }

        private async UniTaskVoid test()
        {
            await UniTask.Delay(3000);
            Log("NIM Test Call inventory");
            GetInventoryClient(0);
            // UpdateAvatar(playerId, new AvatarSet(fakejson));
        }


        private Action<string, List<CollectionAndId>> inventoryUpdateCallBack;
        public void AddInventoryUpdateCallback(Action<string, List<CollectionAndId>> callback)
        {
            inventoryUpdateCallBack += callback;
        }

        private void GetInventoryClient(uint playerId)
        {
            //call Server Game
            NetworkClient.Send(new InventoryUpdateMessage(playerId, "", new InventoryCommunicateData()));
        }

        protected virtual void OnInventoryResponse(InventoryUpdateMessage message)
        {
            Log("NIM Message: " + message.Message);
            //begin event for cleare all nft list and assign maxCount of item will receive;
            if (message.Message == "-begin")
            {
                nftMaxCount = message.Data.Count;
                nftList.Clear();
                OnNftListCount?.Invoke(nftMaxCount);
            }


            //add event for add nft to list;
            if (message.Message == "-add")
            {
                var nft = message.Data;
                nftList.Add(nft);
                OnAdd?.Invoke(nft);
            }


            //end event for finish nftlist data;
            if (message.Message == "-end")
            {
                string list = "";
                foreach (var item in nftList)
                {
                    list += "\n" + (item.Collection + "/" + item.Id);
                }
                OnFinish?.Invoke(nftList);
            }
        }



    }
}
#endif