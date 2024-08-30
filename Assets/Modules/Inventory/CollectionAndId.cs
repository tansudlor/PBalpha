namespace com.playbux.inventory
{
    public struct CollectionAndId : IInfoType<string, string>
    {
        public CollectionAndId(string collection, string id)
        {
            Collection = collection;
            Id = id;
        }

        public string Collection { get; set; }
        public string Id { get; set; }
    }
}
