using UnityEngine;

namespace SpaceTrader.Util {
    public static class UnityExtensions {
        public static Vector2 XY(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.y);
        }

        public static Vector2 XZ(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector3 ToXZ(this Vector2 vector2) {
            return new Vector3(vector2.x, 0, vector2.y);
        }

        public static Rect Encapsulate(this Rect rect, Vector2 point) {
            rect.xMin = Mathf.Min(point.x, rect.xMin);
            rect.yMin = Mathf.Min(point.y, rect.yMin);
            rect.xMax = Mathf.Max(point.x, rect.xMax);
            rect.yMax = Mathf.Max(point.y, rect.yMax);
            return rect;
        }

        public static Rect Expand(this Rect rect, Vector2 distance) {
            rect.xMin -= distance.x;
            rect.xMax += distance.x;
            rect.yMin -= distance.y;
            rect.yMax += distance.y;
            return rect;
        }
    }
}