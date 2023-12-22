using g3;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Volume {
    public static class VolumeEx {
        public static IEnumerable<Vector3> BuildVector3s(this float[] vertices) {
            for(var i = 0; i < vertices.Length; i += 3)
                yield return new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]);
        }

        public static IEnumerable<int> GetTriangleIndexesHaving(this ColoredVertex vertex, IVolume volume) {
            for(var i = 0; i < volume.Triangles.Length; i++)
                if(volume.Triangles[i].Contains(vertex, volume.Vertices))
                    yield return i;
        }

        public static Vector3 CalculateVertexNormal(this ColoredVertex vertex, IVolume volume) {
            var inTriangles = GetTriangleIndexesHaving(vertex, volume);
            if(!inTriangles.Any())
                return Vector3.Zero;
            Vector3 sum = new Vector3(0, 0, 0); 
            foreach (var idx in inTriangles) {
                sum += volume.Triangles[idx].CalculateNormal(volume.Vertices);
            }
            return Vector3.Normalize(sum);
        }

        public static IEnumerable<Vector3> CalculateVertexNormals(this IVolume volume) {
            foreach(var vertex in volume.Vertices)
                yield return CalculateVertexNormal(vertex, volume);
        }

        public static IEnumerable<Vector3> CalculateTriangleNormals(this IVolume volume) {
            foreach(var triangleIndices in volume.Triangles)
                yield return triangleIndices.CalculateNormal(volume.Vertices);
        }

        public static IEnumerable<Triangle> BuildTriangleIndices(this int[] indices) {
            for(var i = 0; i < indices.Length; i += 3)
                yield return new Triangle(indices[i], indices[i + 1], indices[i + 2]);
        }

        public static IEnumerable<ColoredVertex> Vector3ArrayToColoredVertices(this Vector3[] positions) {
            foreach (var position in positions)
            {
                yield return new ColoredVertex(position);
            }
        }

        public static IEnumerable<Vector3d> ColoredVerticesToVector3d(this ColoredVertex[] vertices) {
            foreach (var vertex in vertices)
            {
                yield return vertex.position.ToVector3d();
            }
        }

        public static IEnumerable<Vector3d> TrianglesToVector3d(this Triangle[] triangles) {
            foreach(var triangle in triangles) {
                yield return new Vector3d(triangle.I0, triangle.I1, triangle.I2);
            }
        }

        public static Vector3d ToVector3d(this Vector3 vector) {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }
    }
}