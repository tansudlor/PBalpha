using System;
namespace com.playbux.networking.client.ability
{
    [Serializable]
    public class HotbarData
    {
        public uint current;
        public uint[][] data;
        public HotbarData(uint current, uint[][] data)
        {
            this.current = current;
            this.data = data;
        }
    }
}