using Cysharp.Threading.Tasks;

namespace SETHD.Echo
{
    public interface IAudioChannel
    {
        void Reinitialize();
        UniTask Play(string key, PlayMode playMode);
        UniTask Pause();
        UniTask Stop();
        UniTask Stop(string key);
    }

    public enum PlayMode
    {
        StartOver,
        Transit
    }
}