using System;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace SpaceTrader.Util {
    public static class UnityExtensions {
        public static Vector2 XY(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.y);
        }

        public static Vector2 XZ(this Vector3 vector3) {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector3 ToXY(this Vector2 vector2, float z = 0f) {
            return new Vector3(vector2.x, vector2.y, z);
        }

        public static Vector3 ToXZ(this Vector2 vector2, float y = 0f) {
            return new Vector3(vector2.x, y, vector2.y);
        }

        public static Vector2Int XY(this Vector3Int vector3i) {
            return new Vector2Int(vector3i.x, vector3i.y);
        }

        public static void Deconstruct(this Vector2Int v, out int x, out int y) {
            x = v.x;
            y = v.y;
        }

        public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z) {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static void Deconstruct(this Vector2 v, out float x, out float y) {
            x = v.x;
            y = v.y;
        }

        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z) {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static void Deconstruct(this Vector4 v, out float x, out float y, out float z, out float w) {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
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

        public static Vector2 Rotate90CW(this Vector2 vector) {
            var y = vector.x * -1;
            var x = vector.y;

            return new Vector2(x, y);
        }

        public static Vector2 Rotation90CCW(this Vector2 vector) {
            var x = vector.y * -1;
            var y = vector.x;

            return new Vector2(x, y);
        }

        public static Rect Encapsulate(this Rect rect, Vector2 point) {
            rect.xMin = Mathf.Min(point.x, rect.xMin);
            rect.yMin = Mathf.Min(point.y, rect.yMin);
            rect.xMax = Mathf.Max(point.x, rect.xMax);
            rect.yMax = Mathf.Max(point.y, rect.yMax);
            return rect;
        }

        public static void Encapsulate(this ref BoundsInt bounds, Vector3Int point) {
            var min = bounds.min;
            var max = bounds.max;

            min.x = Mathf.Min(point.x, min.x);
            min.y = Mathf.Min(point.y, min.y);
            min.z = Mathf.Min(point.z, min.z);
            max.x = Mathf.Max(point.x, max.x);
            max.y = Mathf.Max(point.y, max.y);
            max.z = Mathf.Max(point.z, max.z);

            bounds.min = min;
            bounds.max = max;
        }

        public static Rect Expand(this Rect rect, Vector2 distance) {
            rect.xMin -= distance.x;
            rect.xMax += distance.x;
            rect.yMin -= distance.y;
            rect.yMax += distance.y;
            return rect;
        }

        public static T GetOrAddComponent<T>(this Component component)
            where T : Component {
            return GetOrAddComponent<T>(component.gameObject);
        }

        public static T GetOrAddComponent<T>(this GameObject obj)
            where T : Component {
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

        public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive)
            where T : class {
            if (!gameObject.TryGetComponentInParent<T>(out var result, includeInactive)) {
                result = null;
            }

            return result;
        }

        public static T GetComponentInParent<T>(this Component component, bool includeInactive) {
            return component.gameObject.GetComponentInParent<T>(includeInactive);
        }

        public static bool TryGetComponentInParent<T>(
            this GameObject gameObject,
            out T result,
            bool includeInactive = false
        )
            where T : class {
            var next = gameObject.transform;
            do {
                var current = next;
                next = next.parent;

                if (!current.gameObject.activeInHierarchy && !includeInactive) {
                    continue;
                }

                if (current.TryGetComponent<T>(out var component)) {
                    result = component;
                    return true;
                }
            } while (next);

            result = null;
            return false;
        }

        public static bool TryGetComponentInParent<T>(
            this Component component,
            out T result,
            bool includeInactive = false
        )
            where T : class {
            return component.gameObject.TryGetComponentInParent(out result, includeInactive);
        }

        public static string GetSerializedPropertyName(this Type type, string memberName) {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (type.GetField(memberName, bindingFlags) != null) {
                return memberName;
            }

            if (type.GetProperty(memberName, bindingFlags) != null) {
                var backingFieldName = $"<{memberName}>k__BackingField";
                if (type.GetField(backingFieldName, bindingFlags) != null) {
                    return backingFieldName;
                }
            }

            return null;
        }

        public static unsafe void SetPositions(this LineRenderer lineRenderer, ReadOnlySpan<Vector3> positions) {
            var safety = AtomicSafetyHandle.Create();
            try {
                fixed (Vector3* positionsPtr = positions) {
                    var slice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<Vector3>(
                        positionsPtr,
                        UnsafeUtility.SizeOf<Vector3>(),
                        positions.Length
                    );
                    NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref slice, safety);

                    lineRenderer.SetPositions(slice);
                }
            } finally {
                AtomicSafetyHandle.Release(safety);
            }
        }
    }
}
