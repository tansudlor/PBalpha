using Newtonsoft.Json;

namespace com.playbux.inventory
{
    public class NftImage
    {
        public string valueType { get; set; }
    }

    public interface IInfoType<COLLECTION_TYPE, ID_TYPE>
    {
        ID_TYPE Id { get; set; }
        COLLECTION_TYPE Collection { get; set; }
    }
    public class NftInfo : IInfoType<string, string>
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Type { get; set; }
        [JsonProperty("Costume Set")]
        public string CostumeSet { get; set; }
        public string Collection { get; set; }
        [JsonProperty("Side Story")]
        public string Description { get; set; }
    }

}
