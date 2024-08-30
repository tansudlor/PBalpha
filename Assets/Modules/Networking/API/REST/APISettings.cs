using System;
using com.playbux.utilis.network;

namespace com.playbux.network.api.rest
{
    [Serializable]
    public class APISettings
    {
        public RemoteConfigString domain;
        public RemoteConfigInt apiPort;
        public RemoteConfigString apiVersion;
    }
}