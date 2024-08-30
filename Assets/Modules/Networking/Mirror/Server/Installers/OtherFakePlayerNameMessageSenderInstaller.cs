using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class OtherFakePlayerNameMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<FakePlayerNameChangeMessage>, FakePlayerNameChangeMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }
}