using System;
using Mirror;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public class TransportSettings
    {
        public Transport defaultTransport;

        public Transport[] Transports;
    }
}