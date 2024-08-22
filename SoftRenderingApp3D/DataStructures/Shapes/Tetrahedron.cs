using SoftRenderingApp3D.DataStructures.Drawables;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;
using System.Numerics;

namespace SoftRenderingApp3D.DataStructures.Shapes
{
    public static class Tetrahedron
    {
        private static readonly Vector3[] _vertices = {
            new Vector3(1, 1, 1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(1, -1, -1)
        };

        private static readonly Facet[] _triangleIndices = new[] {
                0, 1, 2,
                0, 2, 3,
                0, 3, 1,
                1, 3, 2
            }
            .BuildTriangleIndices();

        private static readonly Vector2[] _textureCoordinates = {
            new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f)
        };

        private static readonly ColorRGB[] _triangleColors = {
            new ColorRGB(255,192,192), new ColorRGB(255,128,128),
            new ColorRGB(255,64,64), new ColorRGB(255,0, 0)
        };

        public static IDrawable GetDrawable()
        {
            var tetrahedron = new Mesh(_vertices, _triangleIndices, null, _textureCoordinates);
            var translateToOrigin = Matrix4x4.CreateTranslation(-new Vector3(.5f, .5f, .5f));
            tetrahedron.Transform(translateToOrigin);
        
            var triangleColorMaterial = new FacetColorMaterial(_triangleColors);
        
            return new Drawable(tetrahedron, triangleColorMaterial);
        }
    }
}
