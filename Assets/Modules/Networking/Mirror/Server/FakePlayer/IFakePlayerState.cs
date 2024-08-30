namespace com.playbux.networking.mirror.server.fakeplayer
{
    public interface IFakePlayerState
    {
        FakePlayerStateEnum StateEnum { get; }
        void Perform();
        void Exit();
    }
}