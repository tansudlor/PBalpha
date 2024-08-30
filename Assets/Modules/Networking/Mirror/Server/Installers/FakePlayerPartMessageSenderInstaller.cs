using com.playbux.networking.mirror.message;
namespace com.playbux.networking.mirror.server
{
    public class FakePlayerPartMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<FakePlayerPartMessage>, FakePlayerPartMessage>
    {
        public override void AdditionalBindings()
        {
            
        }
    }
}
