using Mirror;

//namespace com.playbux.networking.mirror.core
namespace com.playbux.identity
{
    public interface ICredentialProvider
    {
        void Update();
        void OnPlayerAuthenticated(string name, uint netId);
        void OnPlayerDisconnected(string name);
        string GetData(NetworkIdentity identity);
        NetworkIdentity GetData(string name);

    }
}