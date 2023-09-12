using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D {
    public class Volume {
        public Volume(Vector3[] vertices, Triangle[] triangleIndices, Vector3[] vertexNormals = null) {
            Vertices = vertices;
            Triangles = triangleIndices;
            NormVertices = vertexNormals ?? this.CalculateVertexNormals().ToArray();

            Scale = Vector3.One;
        }
        public Vector3[] Vertices { get; }

        public Triangle[] Triangles { get; }

        public Vector3 Centroid { get; }

        public Rotation3D Rotation { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public Vector3[] NormVertices { get; set; }

        public Matrix4x4 WorldMatrix() =>
            Matrix4x4.CreateFromYawPitchRoll(Rotation.YYaw, Rotation.XPitch, Rotation.ZRoll) *
            Matrix4x4.CreateTranslation(Position) *
            Matrix4x4.CreateScale(Scale);
    }
}
