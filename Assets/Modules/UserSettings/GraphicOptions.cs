using System;
using AYellowpaper.SerializedCollections;

namespace com.playbux.settings
{
    [Serializable]
    public class GraphicOptions
    {
        public uint targetFps = 30;
        public PCAppMode[] pcAppMode;
        public int[] msaas;
        public SerializedDictionary<int, int> aspectRatios = new SerializedDictionary<int, int>();
        public SerializedDictionary<string, GraphicLevel> graphicLevel = new SerializedDictionary<string, GraphicLevel>();
    }
}