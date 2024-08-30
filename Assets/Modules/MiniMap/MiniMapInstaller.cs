
using UnityEngine;
using com.playbux.events;
using Zenject;
using com.playbux.ui;

namespace com.playbux.minimap
{
    [CreateAssetMenu(menuName = "Playbux/UI/MiniMap", fileName = "MiniMap")]
    public class MiniMapInstaller : ScriptableObjectInstaller<MiniMapInstaller>
    {
        [SerializeField]
        private GameObject fullMiniMapPrefab;

        private UICanvas canvas;
        [Inject]
        void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }
        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
#endif
        }
#if !SERVER
        private void BindClientSide()
        {

            Container.Bind<FullMiniMapController>()
                .FromComponentInNewPrefab(fullMiniMapPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle();

            
          
        }
#endif
    }
}

