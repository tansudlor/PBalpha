using System.Linq;
using UnityEngine;
using com.playbux.LOD;
using AYellowpaper.SerializedCollections;

namespace com.playbux.map
{
    public class MapObject : MonoBehaviour
    {
        public string ObjectName => objectName;
        public int Width => width;
        public int Height => height;
        public int GridSize => gridSize;
        public float HighQualityScale => highQualityScale;
        public float MediumQualityScale => mediumQualityScale;
        public float LowQualityScale => lowQualityScale;
        public float DefaultQualityScale => defaultQualityScale;
        public SerializedDictionary<int, LODTexture2D> Textures => textures;

        [SerializeField]
        private string objectName;

        [SerializeField]
        private int width;

        [SerializeField]
        private int height;

        [SerializeField]
        private int gridSize = 256;

        [SerializeField]
        private float highQualityScale;

        [SerializeField]
        private float mediumQualityScale;

        [SerializeField]
        private float lowQualityScale;

        [SerializeField]
        private float defaultQualityScale;

        [SerializeField]
        private SerializedDictionary<int, LODTexture2D> textures;

#if UNITY_EDITOR
        public void SetWidth(int width)
        {
            this.width = width;
        }
        public void SetHeight(int height)
        {
            this.height = height;
        }
        public void SetGridSize(int gridSize)
        {
            this.gridSize = gridSize;
        }
        public void AddTexture(int position, LODTexture2D gridTexture)
        {
            textures.TryAdd(position, gridTexture);
        }

        public void Sort()
        {
            var sorted = textures.OrderBy(pair => pair.Key).ToArray();
            var newDict = new SerializedDictionary<int, LODTexture2D>();
            foreach (var pair in sorted)
            {
                newDict.Add(pair.Key, pair.Value);
            }

            textures.Clear();
            textures = newDict;
        }
#endif
    }

}