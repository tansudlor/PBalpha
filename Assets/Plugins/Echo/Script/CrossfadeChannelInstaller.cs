using UnityEngine;
using Zenject;

namespace SETHD.Echo
{
    public class CrossfadeChannelInstaller : MonoInstaller<CrossfadeChannelInstaller>
    {
        [SerializeField]
        private AudioBank audioBank;

        [SerializeField]
        private GameObject audioSourceContext;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CrossFadeBGMChannel>().AsSingle();
            Container.Bind<AudioBank>().FromInstance(audioBank).AsSingle();
            Container.Bind<Transform>().FromInstance(transform).AsSingle();


            Container.Bind<AudioSourceProvider>().FromSubContainerResolve().ByNewContextPrefab(audioSourceContext).AsCached();
        }
    }


}