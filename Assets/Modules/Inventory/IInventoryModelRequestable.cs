using Mirror;
using System.Collections.Generic;

namespace com.playbux.inventory
{
    public interface IInventoryModelRequestable
    {
        void GetInventory(uint playerId, NetworkConnectionToClient connection);
    }
}
