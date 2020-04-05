using UnityEngine;

namespace SpaceTrader.Util {
    public static class GameObjectGizmosUtility {
        public static void DrawWireMeshGizmos(
            this GameObject root,
            Vector3 pos,
            Quaternion rot,
            Vector3? scale = default
        ) {
            var xform = Matrix4x4.TRS(pos, rot, scale ?? Vector3.one);
            DrawWireMeshGizmos(root, xform);
        }

        public static void DrawWireMeshGizmos(
            this GameObject root,
            Matrix4x4 xform
        ) {
            var origin = xform.MultiplyPoint(Vector3.zero);

            if (root.TryGetComponent(out MeshFilter filter) && filter.sharedMesh) {
                var subMeshCount = filter.sharedMesh.subMeshCount;
                for (var sub = 0; sub < subMeshCount; ++sub) {
                    Gizmos.DrawWireMesh(filter.sharedMesh, sub, origin,
                        xform.rotation, xform.lossyScale);
                }
            }

            if (root.TryGetComponent(out SpriteRenderer _)) {
                Gizmos.DrawWireCube(origin, xform.lossyScale);
            }

            for (var i = 0; i < root.transform.childCount; i += 1) {
                var child = root.transform.GetChild(i);
                var childXform = Matrix4x4.TRS(
                    child.localPosition,
                    child.localRotation,
                    child.localScale);

                DrawWireMeshGizmos(child.gameObject, childXform);
            }
        }
    }
}