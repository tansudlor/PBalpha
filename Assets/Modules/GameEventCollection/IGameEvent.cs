
using Cysharp.Threading.Tasks;
using Mirror;

namespace com.playbux.gameeventcollection
{
    public interface IGameEvent
    {
        long EndPrepare { get; }
        UniTaskVoid RunEvent();

        void OnClientConnected(NetworkConnectionToClient connection);

        void CloseEvent();
    }
}
