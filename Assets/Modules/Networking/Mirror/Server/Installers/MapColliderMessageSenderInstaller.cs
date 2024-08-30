using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.map
{
    public class MapColliderMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<MapColliderUpdateMessage>, MapColliderUpdateMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }
}