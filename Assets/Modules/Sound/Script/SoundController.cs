using SETHD.Echo;
using com.playbux.events;
using System.Collections.Generic;

namespace com.playbux.sound
{
    public class SoundController
    {
        private readonly Dictionary<AudioChannelKey, IAudioChannel> audioChannels;

        public SoundController(List<AudioChannelFacade> audioChannelFacades)
        {
            audioChannels = new Dictionary<AudioChannelKey, IAudioChannel>();

            for (int i = 0; i < audioChannelFacades.Count; i++)
                audioChannels.Add(audioChannelFacades[i].AudioChannelKey, audioChannelFacades[i].AudioChannel);
        }
        public void OnBGMPlayRequest(BGMPlaySignal signal)
        {
            audioChannels[AudioChannelKey.BGM].Play(signal.key, signal.playMode);
        }
        
        public void OnBGMStopRequest(BGMStopSignal signal)
        {
            audioChannels[AudioChannelKey.BGM].Stop(signal.key);
        }
        
        public void OnBGMStopAllRequest(BGMStopAllSignal signal)
        {
            audioChannels[AudioChannelKey.BGM].Stop();
        }

        public void OnSFXPlayRequest(SFXPlaySignal signal)
        {
            audioChannels[AudioChannelKey.SFX].Play(signal.key, PlayMode.StartOver);
        }
    }
}