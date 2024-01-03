using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Meshes
{
    public interface IPointCloud
    {
        int VertexCount { get; }
        IReadOnlyList<Vector3> Vertices { get; }
        IReadOnlyList<Vector3> VertexNormals { get; }
        IReadOnlyList<Vector2> TexCoordinates { get; }
        void SetVertexPosition(int i, Vector3 position);
        void SetVertexNormal(int i, Vector3 normal);
        void SetVertexTextureCoordinate(int i, Vector2 uv);
        void Transform(Matrix4x4 matrix);
    }
}
