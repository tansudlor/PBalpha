using TMPro;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.client.FATE
{
    public class FATENotificationUIInstaller : MonoInstaller<FATENotificationUIInstaller>
    {
        [SerializeField]
        private TextMeshProUGUI messageText;
        
        public override void InstallBindings()
        {
            Container.Bind<FATENotificationUI>().AsSingle();
            Container.Bind<TextMeshProUGUI>().FromInstance(messageText).AsSingle();
            Container.Bind<Transform>().FromInstance(gameObject.transform).AsSingle();
        }
    }
}