using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client
{
    public class PlayerMoveMessageSenderInstaller : ClientTickMessageSenderInstaller<ClientTickMessageSender<PlayerMoveInputMessage>, PlayerMoveInputMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }

}