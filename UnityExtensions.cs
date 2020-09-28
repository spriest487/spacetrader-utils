using UnityEngine;

namespace SpaceTrader.Util {
    public static class UnityExtensions {
        public static Vector2 XY(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.y);
        }

        public static Vector2 XZ(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector3 ToXY(this Vector2 vector2) {
            return new Vector3(vector2.x, vector2.y, 0);
        }

        public static Vector3 ToXZ(this Vector2 vector2) {
            return new Vector3(vector2.x, 0, vector2.y);
        }

        public static Vector2Int XY(this Vector3Int vector3i) {
            return new Vector2Int(vector3i.x, vector3i.y);
        }

        public static Vector2Int RoundToInt(this Vector2 vector2) {
            var x = Mathf.RoundToInt(vector2.x);
            var y = Mathf.RoundToInt(vector2.y);
            return new Vector2Int(x, y);
        }

        public static Vector3Int RoundToInt(this Vector3 vector2) {
            var x = Mathf.RoundToInt(vector2.x);
            var y = Mathf.RoundToInt(vector2.y);
            var z = Mathf.RoundToInt(vector2.z);
            return new Vector3Int(x, y, z);
        }

        public static Vector2 Rotate(this Vector2 vector2, float degrees) {
            var sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            var cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            var tx = vector2.x;
            var ty = vector2.y;
            vector2.x = cos * tx - sin * ty;
            vector2.y = sin * tx + cos * ty;
            return vector2;
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

        public static T GetOrAddComponent<T>(this Component component) where T : Component {
            return GetOrAddComponent<T>(component.gameObject);
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
            if (!obj.TryGetComponent(out T newComponent)) {
                newComponent = obj.AddComponent<T>();
            }

            return newComponent;
        }

        public static Color GetPixelNearest(this Texture2D texture, float u, float v) {
            var x = Mathf.RoundToInt(Mathf.Clamp(0, texture.width, u * texture.width));
            var y = Mathf.RoundToInt(Mathf.Clamp(0, texture.height, v * texture.height));
            return texture.GetPixel(x, y);
        }

        private static readonly Vector3[] screenCornersBuf = new Vector3[4];

        public static Rect GetScreenRect(this RectTransform rectTransform) {
            rectTransform.GetWorldCorners(screenCornersBuf);
            var min = screenCornersBuf[0];
            var max = screenCornersBuf[2];

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
