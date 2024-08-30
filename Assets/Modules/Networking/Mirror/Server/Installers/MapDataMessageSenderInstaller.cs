using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.map
{
    public class MapDataMessageSenderInstaller : ServerTickMessageSenderInstaller<ServerTickMessageSender<MapDataMessage>, MapDataMessage>
    {
        public override void AdditionalBindings()
        {

        }
    }

}