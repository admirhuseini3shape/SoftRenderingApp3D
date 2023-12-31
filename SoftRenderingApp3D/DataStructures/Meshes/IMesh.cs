using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public interface IMesh : IPointCloud
    {
        IReadOnlyList<Triangle> Triangles { get; }
        IReadOnlyList<ColorRGB> TriangleColors { get; }
    }

    public interface IPointCloud
    {
        IReadOnlyList<Vector3> Vertices { get; }
        IReadOnlyList<Vector3> VertexNormals { get; }
        IReadOnlyList<ColorRGB> VertexColors { get; }
        IReadOnlyList<Vector2> TexCoordinates { get; }
        void SetVertexPosition(int i, Vector3 position);
        void SetVertexNormal(int i, Vector3 normal);
        void SetVertexColor(int i, ColorRGB color);
        void SetVertexTextureCoordinate(int i, Vector2 uv);
    }
}
