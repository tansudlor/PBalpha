using Mirror;
using UnityEngine;
using com.playbux.entities;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.client.fakeplayer;
using com.playbux.networking.mirror.server.fakeplayer;

namespace com.playbux.networking.mirror.entity
{
    public class NetworkFakePlayerEntity : IEntity<FakePlayerIdentity>
    {
        public uint TargetId => identity.Id;
        public FakePlayerIdentity Identity => identity;
        public GameObject GameObject { get; }

        internal readonly FakePlayerIdentity identity;

#if SERVER
        private readonly IFakePlayerServerBehaviour serverBehaviour;

        public NetworkFakePlayerEntity(FakePlayerIdentity identity, GameObject gameObject, IFakePlayerServerBehaviour serverBehaviour)
        {
            GameObject = gameObject;
            this.identity = identity;
            this.serverBehaviour = serverBehaviour;
            GameObject = identity.NetworkIdentity.gameObject;
        }

        public void Initialize()
        {
            serverBehaviour.Initialize();
        }
        public void Dispose()
        {
            serverBehaviour.Dispose();
            NetworkServer.Destroy(GameObject);
        }
#else
        private readonly IFakePlayerClientBehaviour clientBehaviour;

        public NetworkFakePlayerEntity(FakePlayerIdentity identity, GameObject gameObject, IFakePlayerClientBehaviour clientBehaviour)
        {
            GameObject = gameObject;
            this.identity = identity;
            this.clientBehaviour = clientBehaviour;
        }

        public void Initialize()
        {
            //FIXME: Disable when dont have Bot
            clientBehaviour.Initialize();
            GameObject.transform.rotation = Quaternion.Euler(314.505005f,266.942017f,274.14801f);
        }

        public void Dispose()
        {
            clientBehaviour.Dispose();
            Object.Destroy(GameObject);
        }
#endif
    }
}