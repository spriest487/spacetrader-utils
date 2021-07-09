using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Util {
    public class MeshBuilder {
        private class Submesh {
            public List<int> indices;
            public MeshTopology topology;
        }

        private readonly List<Vector3> vertices;
        private readonly List<Vector3> normals;
        private readonly List<Vector2> texCoords0;
        private readonly List<Color> colors;

        private readonly List<Submesh> submeshes;

        public MeshBuilder() {
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.texCoords0 = new List<Vector2>();
            this.colors = new List<Color>();
            
            this.submeshes = new List<Submesh>();
        }

        public int EmitVert(
            Vector3 position,
            Vector3? normal = null,
            Vector2? texCoord0 = null,
            Color? color = null
        ) {
            this.vertices.Add(position);

            if (normal.HasValue) {
                Fill(this.normals, this.vertices.Count - 1);
                this.normals.Add(normal.Value);
            }
            if (texCoord0.HasValue) {
                Fill(this.texCoords0, this.vertices.Count - 1);
                this.texCoords0.Add(texCoord0.Value);
            }
            if (color.HasValue) {
                Fill(this.colors, this.vertices.Count - 1);
                this.colors.Add(color.Value);
            }

            return this.vertices.Count - 1;
        }

        private void EnsureSubmeshExists(int index, MeshTopology topology) {
            while (this.submeshes.Count <= index) {
                this.submeshes.Add(new Submesh { indices = new List<int>(0) });
            }
            
            if (this.submeshes[index].indices.Count > 0) {
                var existingTopology = this.submeshes[index].topology;
                Debug.AssertFormat(existingTopology == topology, "emitting {0} geometry into {1} submesh", topology, existingTopology);
            } else {
                this.submeshes[index].topology = topology;
            }
        }

        public void EmitTri(int submesh, int v0, int v1, int v2) {
            this.EnsureSubmeshExists(submesh, MeshTopology.Triangles);

            var indices = this.submeshes[submesh].indices;
            indices.Add(v0);
            indices.Add(v1);
            indices.Add(v2);
        }
        
        public void EmitQuad(int submesh, int v0, int v1, int v2, int v3, bool emitTris = false) {
            if (emitTris) {
                this.EnsureSubmeshExists(submesh, MeshTopology.Triangles);
                var indices = this.submeshes[submesh].indices;
                indices.Add(v0);
                indices.Add(v1);
                indices.Add(v2);

                indices.Add(v0);
                indices.Add(v2);
                indices.Add(v3);
            } else {
                this.EnsureSubmeshExists(submesh, MeshTopology.Quads);
                var indices = this.submeshes[submesh].indices;
                indices.Add(v0);
                indices.Add(v1);
                indices.Add(v2);
                indices.Add(v3);
            }
        }

        private static void Fill<T>(List<T> list, int size) {
            while (list.Count < size) {
                list.Add(default);
            }
        }

        public void Build(Mesh mesh) {
            mesh.Clear();
            mesh.SetVertices(this.vertices);
            
            if (this.normals.Count != 0) {
                Fill(this.normals, this.vertices.Count);
                mesh.SetNormals(this.normals);
            }
            
            if (this.colors.Count != 0) {
                Fill(this.colors, this.vertices.Count);
                mesh.SetColors(this.colors);
            }

            if (this.texCoords0.Count != 0) {
                Fill(this.texCoords0, this.vertices.Count);
                mesh.SetUVs(0, this.texCoords0);
            }
            
            mesh.subMeshCount = this.submeshes.Count(s => s.indices.Count > 0);
            var submeshIndex = 0;
            foreach (var submesh in this.submeshes) {
                if (submesh.indices.Count == 0) {
                    continue;
                }

                mesh.SetIndices(submesh.indices, submesh.topology, submeshIndex);
                submeshIndex += 1;
            }
        }

        public void Clear() {
            this.vertices.Clear();
            this.colors.Clear();
            this.normals.Clear();
            this.texCoords0.Clear();
            
            foreach (var submesh in this.submeshes) {
                submesh.indices.Clear();
            }
        }
    }
}
