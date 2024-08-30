using UnityEngine;

namespace com.playbux.sorting
{
    public class RendererSortComponentInstaller : BaseSortComponentInstaller<RendererSortComponent>
    {
        [SerializeField]
        private Renderer renderer;

        internal override void BindComponent()
        {
            Container.Bind<Renderer>().FromInstance(renderer).AsSingle();
        }
    }
}