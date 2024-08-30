using Zenject;
using UnityEngine;
using com.playbux.utilis.canvas;
using UnityEngine.Serialization;
using com.playbux.utilis.uitext;

namespace com.playbux.utilis.debug.position
{
    [CreateAssetMenu(menuName = "Playbux/Debug/Create PositionDebugInstaller", fileName = "PositionDebugInstaller", order = 0)]
    public class PositionDebugInstaller : ScriptableObjectInstaller<PositionDebugInstaller>
    {
        [FormerlySerializedAs("debugTextPrefab")]
        [SerializeField]
        private WorldToScreenText worldToScreenTextPrefab;

        [SerializeField]
        private GameObjectContext debugCanvasContext;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PositionDebugDrawer>().AsSingle();
            Container.Bind<DebugCanvas>().FromSubContainerResolve().ByNewContextPrefab(debugCanvasContext).AsSingle();
            Container.BindMemoryPool<WorldToScreenText, WorldToScreenText.Pool>().FromComponentInNewPrefab(worldToScreenTextPrefab).UnderTransformGroup("DebugTexts").AsCached();
        }
    }
}