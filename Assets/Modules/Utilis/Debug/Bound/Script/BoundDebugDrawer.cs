using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using com.playbux.utilis.debug.line;

namespace com.playbux.utilis.debug
{
    public class BoundDebugDrawer
    {
        private DebugLine.Pool linePool;

        private DebugSquare.Pool squarePool;

        private Dictionary<Transform, DebugLine[]> lineDict = new();

        private Dictionary<Transform, DebugSquare[]> squareDict = new();

        public BoundDebugDrawer(DebugLine.Pool linePool, DebugSquare.Pool squarePool)
        {
            this.linePool = linePool;
            this.squarePool = squarePool;
        }

        public void Draw(Transform parent, Vector3[] boundPoints, float scale = 1f)
        {
            var squares = boundPoints.Select(boundPoint => squarePool.Spawn(scale, boundPoint, parent)).ToArray();
            var lines = boundPoints.Select((t, i) =>
            {
                int endIndex = i + 1 >= boundPoints.Length ? 0 : i + 1;
                return DrawLine(parent, t, boundPoints[endIndex], scale);
            }).ToArray();

            lineDict.Add(parent, lines);
            squareDict.Add(parent, squares);
        }

        private DebugLine DrawLine(Transform parent, Vector3 startPosition, Vector3 endPosition, float thickness = 1f)
        {
            return linePool.Spawn(thickness, startPosition, endPosition, parent);
        }
    }
}