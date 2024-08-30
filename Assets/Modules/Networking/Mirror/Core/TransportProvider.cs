using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    public class TransportProvider
    {
        private Transport defaultTransport;
        private readonly Dictionary<string, Transport> availableTransports;

        public TransportProvider(TransportSettings settings)
        {
            defaultTransport = settings.defaultTransport;
            availableTransports = new Dictionary<string, Transport>();

            foreach (var pair in settings.Transports)
            {
                availableTransports.Add(pair.name, pair);
            }
        }

        public Transport Get(string name)
        {
            if (!availableTransports.ContainsKey(name))
            {
#if DEVELOPMENT
                if (name != "default")
                    Debug.Log("Transport key :" + name + " does not exist");
#endif
                
                return defaultTransport;
            }

            return availableTransports[name];
        }
    }
}