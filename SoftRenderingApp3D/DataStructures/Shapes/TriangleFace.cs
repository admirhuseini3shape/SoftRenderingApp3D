using SoftRenderingApp3D.DataStructures.Meshes;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes
{
    public static class TriangleFace
    {
        private static readonly Vector3[] vertices = {
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 0, 1)
        };

        private static readonly Facet[] triangleIndices = new[] { 0, 1, 2 }.BuildTriangleIndices();
        public static Mesh GetMesh()
        {
            var mesh = new Mesh(vertices, triangleIndices);

            var translate = -new Vector3(.5f, .5f, .5f);
            var matrix = Matrix4x4.CreateTranslation(translate);

            mesh.Transform(matrix);

            return mesh;
        }
    }
}
