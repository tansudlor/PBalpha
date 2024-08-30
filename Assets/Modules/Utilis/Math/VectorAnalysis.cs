using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.utilis.math
{
    public static class VectorAnalysis
    {
        public static Vector2 Rotate(this ref Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}
