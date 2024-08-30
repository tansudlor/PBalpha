using UnityEngine;
using Zenject;
using Newtonsoft.Json;
using System.Collections.Generic;
using com.playbux.avatar;
using System.IO;
using System;
using System.Collections;

namespace com.playbux.inventory
{
    public abstract class InventoryModelBase<TCollectionType, TIdType, TDataStructType, TInfoType> : IInventoryModel<TCollectionType, TIdType, TDataStructType, TInfoType>
     where TInfoType : IInfoType<TCollectionType, TIdType>
     where TDataStructType : IInfoType<TCollectionType, TIdType>
    {
        protected Dictionary<TDataStructType, TInfoType> allCollection;
        public InventoryModelBase(TextAsset NFTDatabase)
        {
            allCollection = new Dictionary<TDataStructType, TInfoType>();
            TInfoType[] array = CreateArray(NFTDatabase.text);
            foreach (TInfoType nft in array)
            {
                TDataStructType cid = InstanceKey(nft.Collection, nft.Id);
                allCollection[cid] = nft;
            }
        }

        protected virtual TInfoType[] CreateArray(string NFTDatabase)
        {
            return JsonConvert.DeserializeObject<TInfoType[]>(NFTDatabase);
        }

        public virtual Dictionary<TDataStructType, TInfoType> GetInfo()
        {
            return allCollection;
        }

        protected abstract TDataStructType InstanceKey(TCollectionType collection, TIdType id);

        public void Log(object log)
        {
#if DEVELOPMENT
            try
            {
                Debug.Log(log.ToString());
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
#endif
        }
    }
}
