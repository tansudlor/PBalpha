using com.playbux.utilis.debug.line;
using Zenject;
using UnityEngine;

namespace com.playbux.utilis.debug
{
    [CreateAssetMenu(menuName = "Playbux/Debug/Create BoundDebugInstaller", fileName = "BoundDebugInstaller", order = 0)]
    public class BoundDebugInstaller : ScriptableObjectInstaller<BoundDebugInstaller>
    {
        [SerializeField]
        private DebugLine debugLinePrefab;

        [SerializeField]
        private DebugSquare debugSquarePrefab;

        public override void InstallBindings()
        {
            Container.Bind<BoundDebugDrawer>().AsSingle();
            Container.BindMemoryPool<DebugLine, DebugLine.Pool>().FromComponentInNewPrefab(debugLinePrefab).UnderTransformGroup("DebugLines").AsCached();
            Container.BindMemoryPool<DebugSquare, DebugSquare.Pool>().FromComponentInNewPrefab(debugSquarePrefab).UnderTransformGroup("DebugSquares").AsCached();
        }
    }
}