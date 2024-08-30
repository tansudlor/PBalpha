using Mirror;
using com.playbux.api;
using System.Collections.Generic;

namespace com.playbux.identity
{
    public interface IIdentitySystem
    {

        public IdentityDetail this[uint index]
        {
            get;
        }

        public Dictionary<string, uint> NameReverse { get; set; }

        public bool ContainsKey(uint netId);

#if !SERVER
        public void GetUserdata(NetworkIdentity identity);
#endif

#if SERVER
        public void ProfileUserData(UserProfile userData, uint id);

#endif
    }


}