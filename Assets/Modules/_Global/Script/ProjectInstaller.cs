using Zenject;
using UnityEngine;
using IngameDebugConsole;
using com.playbux.utilis.debug;
using UnityEngine.EventSystems;
using com.playbux.utilis.debug.position;

namespace com.playbux
{
    [CreateAssetMenu(menuName = "Playbux/Create ProjectInstaller", fileName = "ProjectInstaller", order = 0)]
    public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
    {
        [SerializeField]
        private EventSystem eventSystem;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystem).AsSingle().NonLazy();
            Container.BindFactory<GameObject, Transform, GameObject, PrefabFactory>().FromFactory<PrefabToGameObjectFactory>();
            Container.BindFactory<Object, Transform, SpriteRenderer, SpriteRendererFactory>().FromFactory<SpriteRendererFactory>();
            Container.BindFactory<Object, Transform, CompositeCollider2D, CompositeCollider2DFactory>().FromFactory<PrefabToCompositeCollider2DFactory>();

#if DEVELOPMENT
            RuntimeDebugInstallBindings();
#endif
        }

        #region DEVELOPMENT

#if DEVELOPMENT

        [SerializeField]
        private DebugLogManager debugLogManager;

        //NOTE: Install development dependencies
        private void RuntimeDebugInstallBindings()
        {
            BoundDebugInstaller.InstallFromResource(Container);
            PositionDebugInstaller.InstallFromResource(Container);
            Container.Bind<DebugLogManager>().FromComponentInNewPrefab(debugLogManager).AsSingle().NonLazy();
        }

#endif

        #endregion
    }
}