using System;
using UnityEngine;

namespace SpaceTrader.Util {
    public static class GameObjectGizmosUtility {
        public static void DrawWireMeshGizmos(
            this GameObject root,
            Vector3 pos,
            Quaternion rot,
            Vector3? scale = default
        ) {
            var prevMatrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(pos, rot, scale ?? Vector3.one); 
            DrawWireMeshGizmos(root);
            Gizmos.matrix = prevMatrix;
        }

        public static void DrawWireMeshGizmos(this GameObject root) {
            DrawHierarchy(root, static gameObj => {
                if (gameObj.TryGetComponent(out MeshFilter filter) && filter.sharedMesh) {
                    var subMeshCount = filter.sharedMesh.subMeshCount;

                    for (var sub = 0; sub < subMeshCount; ++sub) {
                        Gizmos.DrawWireMesh(filter.sharedMesh, sub);
                    }
                }

                if (gameObj.TryGetComponent(out SpriteRenderer _)) {
                    var origin = Gizmos.matrix.MultiplyPoint(Vector3.zero);
                    Gizmos.DrawWireCube(origin, Gizmos.matrix.lossyScale);
                }
            });
        }

        public static void DrawMeshGizmos(this GameObject root) {
            DrawHierarchy(root, static gameObj => {
                if (!gameObj.TryGetComponent(out MeshFilter filter) || !filter.sharedMesh) {
                    return;
                }

                for (var sub = 0; sub < filter.sharedMesh.subMeshCount; sub += 1) {
                    Gizmos.DrawMesh(filter.sharedMesh, sub);
                }
            });
        }

        private static void DrawHierarchy(GameObject root, Action<GameObject> drawAction) {
            var matrix = Gizmos.matrix;

            drawAction(root);

            for (var i = 0; i < root.transform.childCount; i += 1) {
                var child = root.transform.GetChild(i);
                Gizmos.matrix = matrix * Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);

                DrawHierarchy(child.gameObject, drawAction);
            }

            Gizmos.matrix = matrix;
        }
    }
}
