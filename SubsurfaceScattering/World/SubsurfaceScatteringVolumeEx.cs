using SoftRenderingApp3D;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SubsurfaceScattering.World {
    static class SubsurfaceScatteringVolumeEx {
        public static IEnumerable<Vector3> BuildVector3s(this float[] vertices) {
            for(var i = 0; i < vertices.Length; i += 3)
                yield return new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]);
        }

        public static IEnumerable<int> GetTriangleIndexesHaving(this ColoredVertex vertex, ISubsurfaceScatteringVolume subsurfaceScatteringVolume) {
            for(var i = 0; i < subsurfaceScatteringVolume.Triangles.Length; i++)
                if(subsurfaceScatteringVolume.Triangles[i].Contains(vertex, subsurfaceScatteringVolume.Vertices))
                    yield return i;
        }

        public static Vector3 CalculateVertexNormal(this ColoredVertex vertex, ISubsurfaceScatteringVolume subsurfaceScatteringVolume) {
            var inTriangles = GetTriangleIndexesHaving(vertex, subsurfaceScatteringVolume);
            if(!inTriangles.Any())
                return Vector3.Zero;
            var sum = inTriangles.Select(idx => subsurfaceScatteringVolume.Triangles[idx].CalculateNormal(subsurfaceScatteringVolume.Vertices)).Distinct().Aggregate((v1, v2) => v1 + v2);
            return Vector3.Normalize(sum);
        }

        public static IEnumerable<Vector3> CalculateVertexNormals(this ISubsurfaceScatteringVolume subsurfaceScatteringVolume) {
            foreach(var vertex in subsurfaceScatteringVolume.Vertices)
                yield return CalculateVertexNormal(vertex, subsurfaceScatteringVolume);
        }

        public static IEnumerable<Vector3> CalculateTriangleNormals(this ISubsurfaceScatteringVolume subsurfaceScatteringVolume) {
            foreach(var triangleIndices in subsurfaceScatteringVolume.Triangles)
                yield return triangleIndices.CalculateNormal(subsurfaceScatteringVolume.Vertices);
        }

        public static IEnumerable<Triangle> BuildTriangleIndices(this int[] indices) {
            for(var i = 0; i < indices.Length; i += 3)
                yield return new Triangle(indices[i], indices[i + 1], indices[i + 2]);
        }

        public static IEnumerable<ColoredVertex> Vector3ArrayToColoredVertices(this Vector3[] positions) {
            for (var i = 0; i < positions.Length; i++) {
                yield return new ColoredVertex(positions[i]);
            }
        }
    }
}
