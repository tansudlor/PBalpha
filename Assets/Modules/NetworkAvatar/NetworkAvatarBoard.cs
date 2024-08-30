using com.playbux.avatar;
using Mirror;

namespace com.playbux.networking.networkavatar
{
    public partial class NetworkAvatarBoard : AvatarBoard<uint, NetworkIdentity>, IAvatarBoardRequestable<uint, NetworkIdentity, NetworkConnectionToClient>
    {
        private AvatarSampleSet avatarSampleSet;
        private readonly IAvatarSet defaultAvatarSet;

        public virtual IAvatarSet GetAvatarSet(uint playerId, NetworkConnectionToClient connection = null, bool updateSet = true)
        {

#if SERVER 
            return GetAvatarSetServer(playerId, connection);
#endif

#if !SERVER
            return GetAvatarSetClient(playerId, updateSet);
#endif
        }

        public virtual IAvatarSet GetAvatarSet(NetworkIdentity playerId, NetworkConnectionToClient connection = null, bool updateSet = true)
        {

#if SERVER 
            return GetAvatarSetServer(GetId(playerId), connection);
#endif

#if !SERVER
            return GetAvatarSetClient(GetId(playerId), updateSet);
#endif
        }


        protected override uint GetId(NetworkIdentity referance)
        {
            return referance.netId;
        }

    }
}
