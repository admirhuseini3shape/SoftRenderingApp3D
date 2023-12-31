using SoftRenderingApp3D.DataStructures.Meshes;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes {
    public static class Cube {
        private static readonly Vector3[] _vertices = {
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 0)
        };

        private static readonly Triangle[] _triangleIndices = new[] {
                0, 1, 2, 2, 3, 0, 0, 3, 4,
                4, 5, 0, 0, 5, 6, 6, 1, 0,
                1, 6, 7, 7, 2, 1, 7, 4, 3,
                3, 2, 7, 4, 7, 6, 6, 5, 4 }
            .BuildTriangleIndices();

        private static readonly Vector2[] _textureCoordinates = {
            new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f)
        };

        private static readonly ColorRGB[] _triangleColors =  {
            ColorRGB.Red, ColorRGB.Red,
            ColorRGB.Gray, ColorRGB.Gray,
            ColorRGB.Yellow, ColorRGB.Yellow,
            ColorRGB.Green, ColorRGB.Green,
            ColorRGB.Magenta, ColorRGB.Magenta,
            ColorRGB.Blue, ColorRGB.Blue
        };

        public static Mesh GetMesh() {
            var cube = new Mesh(
                _vertices, _triangleIndices,
                null, null,
                _textureCoordinates, _triangleColors);

            var translateToOrigin = Matrix4x4.CreateTranslation(-new Vector3(.5f, .5f, .5f));

            cube.Transform(translateToOrigin);

            return cube;
        }
    }
}