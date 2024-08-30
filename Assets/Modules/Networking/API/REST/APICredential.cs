using com.playbux.api;

namespace com.playbux.network.api.rest
{
    public class APICredential
    {
        public string AccessToken => TokenUtility.accessTokenKey;
    }
}