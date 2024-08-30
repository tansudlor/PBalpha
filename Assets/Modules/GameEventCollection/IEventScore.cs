using Cysharp.Threading.Tasks;

namespace com.playbux.gameeventcollection
{
    public interface IEventScore
    {
        UniTask SendScore(string uid, string eventname, uint score);
        UniTask<uint> GetScore(string uid, string eventname);
    }
}