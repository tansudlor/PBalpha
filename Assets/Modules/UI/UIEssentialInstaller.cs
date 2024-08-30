using UnityEngine;
using Zenject;
namespace com.playbux.ui
{
    [CreateAssetMenu(menuName = "Playbux/UI/Create UIEssentialInstaller", fileName = "UIEssentialInstaller", order = 0)]
    public class UIEssentialInstaller : ScriptableObjectInstaller<UIEssentialInstaller>
    {
        [SerializeField]
        private UICanvas uiCanvas;

        [SerializeField]
        private GameObject teleportCutoutContext;

        public override void InstallBindings()
        {
            Container.Bind<UICanvas>().FromComponentInNewPrefab(uiCanvas).AsSingle().NonLazy();
#if !SERVER
            Container.Bind<TeleportScreenCutout>().FromSubContainerResolve().ByNewContextPrefab(teleportCutoutContext).AsSingle();
#endif
        }
    }
}