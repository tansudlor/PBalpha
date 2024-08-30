using System;
using Mirror;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public class EnemyIdentity
    {
        public uint Id => networkIdentity.netId;
        public string Name => name;

        public NetworkIdentity NetworkIdentity => networkIdentity;

        [SerializeField]
        private string name;

        [SerializeField]
        private NetworkIdentity networkIdentity;
    }

}