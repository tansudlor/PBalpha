using System;

namespace com.playbux.networking
{
    public class ChangeNetworkSignal
    {
        public NetworkSetting Setting { get; }
        
        public ChangeNetworkSignal(NetworkSetting setting)
        {
            Setting = setting;
        }

    }

    [Serializable]
    public class NetworkSetting
    {
        public string networkAddress;

        public string transportName;

        public NetworkSetting(string networkAddress, string transportName = "default")
        {
            this.networkAddress = networkAddress;
            this.transportName = transportName;
        }
    }
}