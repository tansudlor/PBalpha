#if SERVER
using com.playbux.inventory;
using Mirror;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using Newtonsoft.Json;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.networkinventory
{
    public partial class NetworkInventoryModel
    {
        [Inject]
        private TextAsset[] fakeData;


        public NetworkInventoryModel(IServerNetworkMessageReceiver<InventoryUpdateMessage> messageReceiver, TextAsset NftDatabase) : base(NftDatabase)
        {
            messageReceiver.OnEventCalled += OnInventoryRequire;
            Log("NIM sub");
        }


        private void GetInventoryServer(uint playerId, NetworkConnectionToClient connection)
        {
            //Call Server API
            FAKEAPI(playerId, connection).Forget();
        }

        private void OnInventoryRequire(NetworkConnectionToClient connection, InventoryUpdateMessage message, int channel)
        {
            Log("Server NIM received message: " + message.NetId);
            Log("Server NIM Find NetworkIdentity");
            Log("Require NIM *************************" + connection);
            GetInventoryServer(message.NetId, connection);

        }

        private async UniTaskVoid FAKEAPI(uint playerId, NetworkConnectionToClient connection)
        {
            Log("Server Download Inventory");
            await UniTask.Delay(3000);
            RootObject root = JsonConvert.DeserializeObject<RootObject>(fakeData[0].text);
            connection.Send<InventoryUpdateMessage>(new InventoryUpdateMessage(playerId, "-begin", new InventoryCommunicateData("", "", root.List.Count)));
            foreach (KeyValuePair<string, NftItem> entry in root.List)
            {
                if (entry.Value.Metadata != null)
                {
                    await UniTask.Delay(10);
                    var Nft = new InventoryCommunicateData(entry.Value, entry.Value.Metadata.Attributes);
                    connection.Send<InventoryUpdateMessage>(new InventoryUpdateMessage(playerId, "-add", Nft));
                    //Log(entry.Value.Metadata.Attributes[]);
                }
            }
            connection.Send<InventoryUpdateMessage>(new InventoryUpdateMessage(playerId, "-end", new InventoryCommunicateData()));

        }
    }
}
#endif
