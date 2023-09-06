using SoftRenderingApp3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D {
    public class TexturedVolume : IVolume {
        public TexturedVolume(Vector3[] vertices, Triangle[] triangleIndices, Vector3[] vertexNormals = null, Vector2[] textureCoordinates = null) {
            Vertices = vertices;
            Triangles = triangleIndices;
            NormVertices = vertexNormals ?? this.CalculateVertexNormals().ToArray();
            TextureCoordinates = textureCoordinates;

            Scale = Vector3.One;
        }
        public Vector3[] Vertices { get; }

        public Vector2[] TextureCoordinates { get; }

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
