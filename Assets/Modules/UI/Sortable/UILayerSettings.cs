using System;
namespace com.playbux.ui.sortable
{
    [Serializable]
    public class UILayerSettings
    {
        public int panelPerLayer = 100;
        public UILayerGroup[] layerGroups;
    }
}