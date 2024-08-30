using UnityEngine.Rendering.Universal;
namespace com.playbux.settings
{
    public class GraphicSettingController
    {
        private readonly GraphicOptions defaultOptions;

        private UserGraphicSettings userGraphicSettings;
        private UniversalRenderPipelineAsset urpAsset;

        public GraphicSettingController(UniversalRenderPipelineAsset urpAsset, GraphicOptions defaultOptions)
        {
            this.urpAsset = urpAsset;
        }
    }
}