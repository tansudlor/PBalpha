using System;
using System.Numerics;
namespace com.playbux.map
{
    [Serializable]
    public class Map
    {
        public string name;
        public int width;
        public int height;
        public float offsetX;
        public float offsetY;
        public Chuck[] chucks;
    }

}