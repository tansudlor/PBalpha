using UnityEngine;
using Zenject;

namespace com.playbux.utilis.canvas
{
    public class DebugCanvasInstaller : MonoInstaller<DebugCanvasInstaller>
    {
        [SerializeField]
        private DebugCanvas debugCanvas;

        public override void InstallBindings()
        {
            Container.Bind<DebugCanvas>().FromComponentInNewPrefab(debugCanvas).AsSingle();
        }
    }
}