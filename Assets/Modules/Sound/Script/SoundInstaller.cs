using Zenject;
using UnityEngine;
using com.playbux.events;

namespace com.playbux.sound
{
    [CreateAssetMenu(menuName = "Playbux/Sound/Create SoundInstaller", fileName = "SoundInstaller", order = 0)]
    public class SoundInstaller : ScriptableObjectInstaller<SoundInstaller>
    {
        [SerializeField]
        private GameObject[] audioChannelPrefabs;

        public override void InstallBindings()
        {
            Container.Bind<SoundController>().AsSingle();

            for (int i = 0; i < audioChannelPrefabs.Length; i++)
                Container.Bind<AudioChannelFacade>().FromSubContainerResolve().ByNewContextPrefab(audioChannelPrefabs[i]).AsTransient();

            Container.BindSignal<BGMPlaySignal>().ToMethod<SoundController>(c => c.OnBGMPlayRequest).FromResolveAll();
            Container.BindSignal<BGMStopSignal>().ToMethod<SoundController>(c => c.OnBGMStopRequest).FromResolveAll();
            Container.BindSignal<BGMStopAllSignal>().ToMethod<SoundController>(c => c.OnBGMStopAllRequest).FromResolveAll();
            Container.BindSignal<SFXPlaySignal>().ToMethod<SoundController>(c => c.OnSFXPlayRequest).FromResolveAll();
        }
    }
}