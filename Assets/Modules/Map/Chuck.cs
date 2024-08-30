using UnityEngine;
using com.playbux.LOD;
using AYellowpaper.SerializedCollections;

namespace com.playbux.map
{

    [CreateAssetMenu(menuName = "Playbux/Map/Create Chuck", fileName = "Chuck", order = 0)]
    public class Chuck : ScriptableObject
    {
        public int Width => width;
        public int Height => height;
        public int GridSize => gridSize;
        public float HighQualityScale => highQualityScale;
        public float MediumQualityScale => mediumQualityScale;
        public float LowQualityScale => lowQualityScale;
        public float DefaultQualityScale => defaultQualityScale;
        public SerializedDictionary<int, LODTexture2D> Textures => textures;

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

#region EDITOR
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

        public void AddTexture(int position, LODTexture2D texture2D)
        {
            textures.TryAdd(position, texture2D);
        }

        public void RemoveTexture(int position)
        {
            textures.Remove(position);
        }
#endif
#endregion
    }
}