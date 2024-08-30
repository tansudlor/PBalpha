using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace com.playbux.network.api.rest
{
    public interface IRESTWorker
    {
        UniTask<string> Handle(UnityWebRequest _request);
    }
}