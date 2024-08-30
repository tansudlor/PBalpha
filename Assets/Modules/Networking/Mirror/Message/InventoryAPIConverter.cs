using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace com.playbux.networking.mirror.message
{
    [System.Serializable]
    public struct InventoryCommunicateData
    {
        public readonly string Id;
        public readonly string Collection;
        public readonly int Count;
        public InventoryCommunicateData(NftItem item, List<Attribute> attributes) : this()
        {
            var map = AttributeToMap(attributes);
            Collection = map["Collection"];
            Count = item.Count;
            Id = item.Id;

        }

        public InventoryCommunicateData(string collection = "", string id = "", int count = 0) : this()
        {
            Collection = collection;
            Count = count;
            Id = id;
        }


        private Dictionary<string, string> AttributeToMap(List<Attribute> attributes)
        {
            var result = new Dictionary<string, string>();
            foreach (var attribute in attributes)
            {
                result[attribute.TraitType] = attribute.Value;
            }
            return result;
        }
    }
    //-------------------------------------------------------------------------------
    public class RootObject
    {
        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("list")]
        public Dictionary<string, NftItem> List { get; set; }
    }

    public class NftItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("nftType")]
        public string NftType { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("attributes")]
        public List<Attribute> Attributes { get; set; }
    }

    public class Attribute
    {
        [JsonProperty("trait_type")]
        public string TraitType { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
