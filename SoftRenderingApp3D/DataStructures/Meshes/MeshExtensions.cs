using g3;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes {
    public static class MeshExtensions {
        public static List<Vector3> BuildVector3s(this float[] vertices) {
            var vectorList = new List<Vector3>(vertices.Length / 3);
            for(var i = 0; i < vertices.Length; i += 3) {
                vectorList.Add(new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]));
            }
            return vectorList;
        }

        private static List<int> GetTriangleIndexesHaving(this Vector3 vertex, IMesh mesh) {
            var indexList = new List<int>();
            for(var i = 0; i < mesh.Triangles.Count; i++) {
                if(mesh.Triangles[i].Contains(vertex, mesh.Vertices)) {
                    indexList.Add(i);
                }
            }
            return indexList;
        }

        public static Vector3 CalculateVertexNormal(this Vector3 vertex, IMesh mesh) {
            var inTriangles = GetTriangleIndexesHaving(vertex, mesh);
            if(!inTriangles.Any()) {
                return Vector3.Zero;
            }

            var sum = new Vector3(0, 0, 0);
            foreach(var idx in inTriangles) {
                sum += mesh.Triangles[idx].CalculateNormal(mesh.Vertices);
            }

            return Vector3.Normalize(sum);
        }

        public static Vector3[] CalculateVertexNormals(this IMesh mesh) {
            var result = new Vector3[mesh.Vertices.Count];
            for(var i = 0; i < result.Length; i++) {
                result[i] = CalculateVertexNormal(mesh.Vertices[i], mesh);
            }
            return result;
        }

        public static List<Vector3> CalculateTriangleNormals(this IMesh mesh) {
            var result = new List<Vector3>();
            for(var i = 0; i < mesh.Triangles.Count; i++) {
                result.Add(mesh.Triangles[i].CalculateNormal(mesh.Vertices));
            }
            return result;
        }


        public static Triangle[] BuildTriangleIndices(this int[] indices) {
            var result = new Triangle[indices.Length / 3];
            for(var i = 0; i < indices.Length; i += 3)
                result[i / 3] = new Triangle(indices[i], indices[i + 1], indices[i + 2]);
            return result;
        }

        public static IEnumerable<Vector3d> ToVector3d(this IEnumerable<Triangle> triangles) {
            foreach(var triangle in triangles) {
                yield return new Vector3d(triangle.I0, triangle.I1, triangle.I2);
            }
        }

        public static Vector3d ToVector3d(this Vector3 vector) {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }
    }
}