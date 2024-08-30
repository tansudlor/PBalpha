using TMPro;
using Zenject;
using UnityEngine;

namespace com.playbux.utilis.uitext
{
    public class UITextInformationInstaller : MonoInstaller<UITextInformationInstaller>
    {
        [SerializeField]
        private TextMeshProUGUI tmp;

        [Inject]
        private UITextInformationSettings settings;

        public override void InstallBindings()
        {
            Container.Bind<UITextInformation>().AsSingle();
            Container.Bind<TextMeshProUGUI>().FromInstance(tmp).AsSingle();
            Container.Bind<UITextInformationSettings>().FromInstance(settings).AsSingle();
        }
    }
}