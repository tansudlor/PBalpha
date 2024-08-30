using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class PlayerUpdatePositionMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<PlayerUpdatePositionMessage>, PlayerUpdatePositionMessage>
    {
        public override void AdditionalBindings()
        {
            
        }
    }
}