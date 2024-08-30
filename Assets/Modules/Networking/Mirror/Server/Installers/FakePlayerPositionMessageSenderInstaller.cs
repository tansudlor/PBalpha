using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class FakePlayerPositionMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<FakePlayerPositionMessage>, FakePlayerPositionMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }
}