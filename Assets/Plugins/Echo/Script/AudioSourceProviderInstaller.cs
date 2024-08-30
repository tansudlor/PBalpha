using Zenject;
using UnityEngine;
using UnityEngine.Audio;

namespace SETHD.Echo
{
    public class AudioSourceProviderInstaller : MonoInstaller<AudioSourceProviderInstaller>
    {
        [Inject]
        private Transform group;

        [SerializeField]
        private AudioMixerGroup mixer;

        public override void InstallBindings()
        {
            Container.Bind<AudioSourceProvider>().AsSingle();
            Container.Bind<Transform>().FromInstance(group).AsSingle();
            Container.Bind<AudioMixerGroup>().FromInstance(mixer).AsSingle();
            Container.BindFactory<AudioSource, AudioSourceFactory>().FromNewComponentOnNewGameObject().UnderTransform(group);
        }
    }
}