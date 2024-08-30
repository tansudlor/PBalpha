using System;
using Zenject;
using Mirror;
using UnityEngine;
using com.playbux.entities;

#if SERVER
using com.playbux.networking.mirror.server;
#else
using com.playbux.networking.mirror.client;
#endif

namespace com.playbux.networking.mirror.entity
{
    [Serializable]
    public enum PlayerRole
    {
        DPS,
        Healer,
        Tank
    }

    public class NetworkPlayerEntity : IEntity<NetworkIdentity>
    {
        public uint TargetId { get; } //TODO: need player targeting system
        public PlayerRole PlayerRole => playerRole;
        public NetworkIdentity Identity => networkIdentity;
        public GameObject GameObject { get; }

#if !SERVER

        private readonly IClientBehaviour.Factory clientFactory;

        private IClientBehaviour clientBehaviour;

        public NetworkPlayerEntity(GameObject gameObject, NetworkIdentity networkIdentity, IClientBehaviour.Factory clientFactory)
        {
            this.clientFactory = clientFactory;
            this.networkIdentity = networkIdentity;
            GameObject = gameObject;
#if DEVELOPMENT
            Debug.Log("Player Entity created");
#endif
        }

        public void Initialize()
        {
            clientBehaviour = clientFactory.Create(networkIdentity.isOwned);
#if DEVELOPMENT
            Debug.Log("Player Entity initialized");
#endif
        }

        public void Dispose()
        {
            clientBehaviour.Dispose();
        }
#endif

        private readonly NetworkIdentity networkIdentity;

        private PlayerRole playerRole = PlayerRole.DPS;

#if SERVER
        private readonly IServerBehaviour serverBehaviour;

        public NetworkPlayerEntity(GameObject gameObject, NetworkIdentity networkIdentity, IServerBehaviour serverBehaviour)
        {
            GameObject = gameObject;
            this.networkIdentity = networkIdentity;
            this.serverBehaviour = serverBehaviour;
#if DEVELOPMENT
            Debug.Log("Player Entity created");
#endif
        }

        public void Initialize()
        {
            serverBehaviour.Initialize();
#if DEVELOPMENT
            Debug.Log("Player Entity initialized");
#endif
        }

        public void Dispose()
        {
            serverBehaviour.Dispose();
        }
#endif

        public class Factory : PlaceholderFactory<GameObject, IEntity<NetworkIdentity>>
        {

        }
    }
}