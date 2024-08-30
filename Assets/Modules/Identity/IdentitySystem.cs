using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.identity
{
#if SERVER
    public partial class IdentitySystem : ICredentialProvider, IIdentitySystem
#else
    public partial class IdentitySystem : ICredentialProvider, IIdentitySystem
#endif
    {

        public IdentityDetail this[uint index]
        {
            get
            {
                if (index == 0)
                {
                    Debug.LogWarning("NetId isn't assignd");
                    return null;
                }

                if (!identity.ContainsKey(index))
                {
#if DEVELOPMENT
                     Debug.Log("create key");
#endif

                    identity[index] = new IdentityDetail();

                }
                return identity[index];
            }
        }
        public Dictionary<string, uint> NameReverse { get => nameReverse; set => nameReverse = value; }

        private Dictionary<uint, IdentityDetail> identity;

        private Dictionary<string, uint> nameReverse;

        public bool ContainsKey(uint netId)
        {
            return identity.ContainsKey(netId);
        }
    }
}