using UnityEngine;

namespace com.playbux.utilis
{
    public static class Helper
    {
        public static bool IsIn(this Vector3[] vertices, Vector3 point, float[] edgeSlopes)
        {
            bool isInside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if (((vertices[i].z > point.z) != (vertices[j].z > point.z)) &&
                    (point.x < (point.z - vertices[i].z) * edgeSlopes[i] + vertices[i].x))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        public static Vector3 ToDirection(this Vector2 input)
        {
            // return new Vector3(input.x - input.y, 0, input.y + input.x);
            input.x *= -1;
            return input;
        }

        public static Vector3 RotateWithPivot(this Vector3 vector, Vector3 origin, Vector3 axis, float angle)
        {
            return origin + Quaternion.AngleAxis(angle, axis) * (vector - origin);
        }

        public static Vector3 ScaleFromPivot(this Vector3 vector, Vector3 origin, Vector3 scale)
        {
            vector.x = origin.x + (vector.x - origin.x) * scale.x;
            vector.y = origin.y + (vector.y - origin.y) * scale.y;
            vector.z = origin.z + (vector.z - origin.z) * scale.z;
            return vector;
        }
    }

}