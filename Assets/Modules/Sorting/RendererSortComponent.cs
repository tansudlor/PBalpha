using UnityEngine;

namespace com.playbux.sorting
{
    public class RendererSortComponent : ISortComponent
    {
        public int SortingOrder => renderer.sortingOrder;

        private readonly Renderer renderer;
        public RendererSortComponent(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public void Sort(int order) => renderer.sortingOrder = order;
    }
}