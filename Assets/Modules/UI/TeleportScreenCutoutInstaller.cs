using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace com.playbux.ui
{
    public class TeleportScreenCutoutInstaller : MonoInstaller<TeleportScreenCutoutInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<TeleportScreenCutout>().AsSingle();
            Container.Bind<Image>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<RectTransform>().FromComponentOn(gameObject).AsSingle();
        }
    }
}