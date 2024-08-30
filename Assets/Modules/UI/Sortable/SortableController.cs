using com.playbux.utilis;
using System.Collections.Generic;

namespace com.playbux.ui.sortable
{
    public class SortableController
    {
        private Dictionary<UILayerGroup, int> layerSortingOrderConstant;
        private Dictionary<UILayerGroup, DarumaHashStack<ISortableUI>> stacks;

        public SortableController(UILayerSettings settings)
        {
            layerSortingOrderConstant = new Dictionary<UILayerGroup, int>(settings.layerGroups.Length);
            stacks = new Dictionary<UILayerGroup, DarumaHashStack<ISortableUI>>(settings.layerGroups.Length);

            for (int i = 0; i < settings.layerGroups.Length; i++)
            {
                stacks[settings.layerGroups[i]] = new DarumaHashStack<ISortableUI>();
                layerSortingOrderConstant[settings.layerGroups[i]] = i * settings.panelPerLayer;
            }
        }

        public void Add(UILayerGroup uiLayerGroup, ISortableUI ui)
        {
            stacks[uiLayerGroup].Add(ui);
            ui.SetOrder(layerSortingOrderConstant[uiLayerGroup] + stacks[uiLayerGroup].Count - 1);
        }

        public void Swap(UILayerGroup uiLayerGroup, int position, ISortableUI ui)
        {
            stacks[uiLayerGroup].Swap(position, ui);
        }

        public void Remove(UILayerGroup uiLayerGroup, ISortableUI ui)
        {
            if (!stacks.ContainsKey(uiLayerGroup))
                return;

            stacks[uiLayerGroup].Hit(ui);
            var sortables = stacks[uiLayerGroup].GetAll();
            for (int i = 0; i < sortables.Length; i++)
                sortables[i].SetOrder(layerSortingOrderConstant[uiLayerGroup] + i);
        }
    }
}