using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class OtherPlayerUpdatePositionMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<OtherPlayerUpdatePositionMessage>, OtherPlayerUpdatePositionMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }

}