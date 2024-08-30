using Zenject;
using UnityEngine;
using com.playbux.utilis.uitext;

namespace com.playbux.utilis.fps
{
    public class FPSCounterInstaller : MonoInstaller<FPSCounterInstaller>
    {
        [SerializeField]
        private UITextInformationSettings settings;

        [SerializeField]
        private UITextInformationInstaller uiTextInformationInstaller;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FPSCounter>().AsSingle();
            Container.Bind<UITextInformationSettings>().FromInstance(settings).AsSingle();
            Container.BindFactory<UITextInformation, UITextInformation.Factory>().FromSubContainerResolve().ByNewContextPrefab(uiTextInformationInstaller).AsSingle();

            Container.DeclareSignal<FPSCounterEnabled>();
            Container.BindSignal<FPSCounterEnabled>().ToMethod<FPSCounter>(counter => counter.FPSCounterEnableSignal).FromResolveAll();
        }
    }
}