using System;
using UnityEngine;
namespace com.playbux.map
{
    public interface IMapController
    {
        public event Action<string> OnCreated;
        int Width { get; }
        int Height { get; }
        float CartesianWidth { get; }
        float CartesianHeight { get; }
        Vector2 Offset { get; }
        void Initialize();
        void Dispose();
        int PositionToGridIndex(Vector2 position);
        int[] GetVariableSizeIndicesAroundCenter(int centerIndex, int gridWidth, int gridHeight);
    }
}