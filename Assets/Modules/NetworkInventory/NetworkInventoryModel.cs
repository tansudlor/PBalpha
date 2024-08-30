using com.playbux.inventory;
using com.playbux.networking.mirror.message;
using Mirror;
using System;
using System.Collections.Generic;

namespace com.playbux.networking.networkinventory
{

    public partial class NetworkInventoryModel : InventoryModel, IInventoryModelRequestable
    {
        public Action<int> OnNftListCount;
        public Action<InventoryCommunicateData> OnAdd;
        public Action<List<InventoryCommunicateData>> OnFinish;
        public void GetInventory(uint playerId, NetworkConnectionToClient connection = null)
        {

#if SERVER
            GetInventoryServer(playerId, connection);
#endif
#if !SERVER
            GetInventoryClient(playerId);
#endif
        }
    }
}