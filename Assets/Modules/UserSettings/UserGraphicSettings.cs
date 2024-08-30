using System;
namespace com.playbux.settings
{
    [Serializable]
    public class UserGraphicSettings
    {
        public uint targetFps = 30;
        public PCAppMode pcAppMode = PCAppMode.Fullscreen;
        public GraphicLevel graphicLevel = GraphicLevel.High;
        public int msa = 2;
        public int aspectRatios;
    }
}