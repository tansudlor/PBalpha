using SETHD.Echo;
namespace com.playbux.sound
{
    public class AudioChannelFacade
    {
        public AudioChannelKey AudioChannelKey { get; }
        public IAudioChannel AudioChannel { get; }

        public AudioChannelFacade(AudioChannelKey audioChannelKey, IAudioChannel audioChannel)
        {
            AudioChannelKey = audioChannelKey;
            AudioChannel = audioChannel;
        }
    }
}