using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.inventory
{
    public class InventoryModel : InventoryModelBase<string, string, CollectionAndId, NftInfo>
    {
        protected Dictionary<string, Dictionary<CollectionAndId, NftInfo>> groupOfType;
        // protected Dictionary<string, Dictionary<CollectionAndId, NftInfo>> groupOfType;
        public InventoryModel(TextAsset NFTDatabase) : base(NFTDatabase)
        {
            groupOfType = new Dictionary<string, Dictionary<CollectionAndId, NftInfo>>();
            foreach (var item in GetInfo())
            {

                if (!groupOfType.ContainsKey(item.Value.Type.ToLower()))
                {
                    groupOfType[item.Value.Type.ToLower()] = new Dictionary<CollectionAndId, NftInfo>();
                }
                groupOfType[item.Value.Type.ToLower()][new CollectionAndId(item.Value.Collection, item.Value.Id)] = item.Value;
            }

            Log("Types of Items : " + groupOfType.Count);
            Log("Types Are : " + string.Join(",", groupOfType.Keys));
        }

        protected override CollectionAndId InstanceKey(string collection, string id)
        {
            return new CollectionAndId(collection, id);
        }

    }
}
