using System.Numerics;

namespace SoftRenderingApp3D {
    public interface IVolume {
        Rotation3D Rotation { get; set; }
        Vector3 Position { get; set; }
        Vector3 Scale { get; }

        ColorRGB[] TriangleColors { get; }
        Triangle[] Triangles { get; }
        ColoredVertex[] Vertices { get; }
        Vector3[] NormVertices { get; }
        Vector2[] TexCoordinates { get; }

        Matrix4x4 WorldMatrix();
    }
}