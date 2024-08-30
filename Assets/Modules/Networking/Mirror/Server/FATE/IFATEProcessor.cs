using com.playbux.FATE;

namespace com.playbux.networking.mirror.server
{
    public interface IFATEProcessor
    {
        void Start(FATEData data);

        void End();
    }
}