using System.Linq;
using System.Numerics;

namespace SoftRenderingApp3D {
    public class Volume : IVolume {
        public Volume(Vector3[] vertices, Triangle[] triangleIndices, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null, ColorRGB[] triangleColors = null) {
            Vertices = vertices;
            Triangles = triangleIndices;

            NormVertices = vertexNormals ?? this.CalculateVertexNormals().ToArray();
            TexCoordinates = textureCoordinates;
            TriangleColors = triangleColors ?? Enumerable.Repeat(ColorRGB.Gray, Triangles.Length).ToArray();

            Scale = Vector3.One;
        }

        public Vector3[] Vertices { get; }

        public Vector2[] TexCoordinates { get; }

        public ColorRGB[] TriangleColors { get; }

        public Triangle[] Triangles { get; }

        // public Vector3 Centroid { get; } Not used anywhere yet.

        public Rotation3D Rotation { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public Vector3[] NormVertices { get; set; }

        public Matrix4x4 WorldMatrix() {
            
            Rotation3D rotation = Rotation3D.FromEulerAngles(.5f, .2f, .1f); // Create
            Quaternion quaternion = rotation.Quaternion; // Extract the Quaternion from Rotation3D object
            
            // Convert the quaternion rotation to a rotation matrix
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(quaternion);

            // Create the translation and scale matrices
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(Position);
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(Scale);

            // Combine the rotation, translation, and scale matrices
            return scaleMatrix * rotationMatrix * translationMatrix;
        }
    }
}
