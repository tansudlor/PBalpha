using UnityEngine;
using Zenject;
namespace com.playbux.sound
{
    public class PlaybuxAudioChannelInstaller : MonoInstaller<PlaybuxAudioChannelInstaller>
    {
        [SerializeField]
        private AudioChannelKey audioChannelKey;

        public override void InstallBindings()
        {
            Container.Bind<AudioChannelFacade>().AsSingle();
            Container.Bind<AudioChannelKey>().FromInstance(audioChannelKey).AsSingle();
        }
    }
}