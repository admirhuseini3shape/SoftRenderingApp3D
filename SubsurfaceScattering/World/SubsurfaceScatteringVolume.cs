using SoftRenderingApp3D;
using System.Linq;
using System.Numerics;

namespace SubsurfaceScattering.World {
    public class SubsurfaceScatteringVolume : ISubsurfaceScatteringVolume {
        public SubsurfaceScatteringVolume(ColoredVertex[] vertices, Triangle[] triangleIndices, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null, ColorRGB[] triangleColors = null) {
            Vertices = vertices;
            Triangles = triangleIndices;

            NormVertices = vertexNormals ?? this.CalculateVertexNormals().ToArray();
            TexCoordinates = textureCoordinates;
            TriangleColors = null;

            Scale = Vector3.One;
        }

        public void InitializeTrianglesColor(ColorRGB color) {
            TriangleColors = Enumerable.Repeat(color, Triangles.Length).ToArray();

        }

        public ColoredVertex[] Vertices { get; }

        public Vector2[] TexCoordinates { get; }

        public ColorRGB[] TriangleColors { get; private set; }


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
