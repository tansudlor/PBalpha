using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client
{
    public class MoveCommandMessageSenderInstaller : ClientTickMessageSenderInstaller<ClientTickMessageSender<MoveCommandMessage>, MoveCommandMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }
}