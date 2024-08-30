using com.playbux.avatar;
using System.Collections.Generic;

namespace com.playbux.inventory
{
    public interface IInventoryModel<COLLECTION_TYPE, ID_TYPE, DATASTRUCT_TYPE, INFO_TYPE> : IReportable
    where INFO_TYPE : IInfoType<COLLECTION_TYPE, ID_TYPE>
    where DATASTRUCT_TYPE : IInfoType<COLLECTION_TYPE, ID_TYPE>
    {
        Dictionary<DATASTRUCT_TYPE, INFO_TYPE> GetInfo();
    }
}
