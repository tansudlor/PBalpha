using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace SETHD.Echo
{
    public class AudioChannelMonoFacade : MonoBehaviour
    {
        private IAudioChannel audioChannel;

        [Inject]
        public void Construct(IAudioChannel audioChannel)
        {
            this.audioChannel = audioChannel;
        }

        public void Play(string key)
        {
            audioChannel.Play(key, PlayMode.Transit).Forget();
        }
    }
}